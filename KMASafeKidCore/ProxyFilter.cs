using CitadelCore.Extensions;
using CitadelCore.IO;
using CitadelCore.Logging;
using CitadelCore.Net.Http;
using CitadelCore.Net.Proxy;
using CitadelCore.Windows.Net.Proxy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WindowsFirewallHelper;

namespace KMASafeKidCore
{
    public class ProxyFilter
    {
        private static byte[] s_blockPageBytes;

        private static readonly ushort s_standardHttpPortNetworkOrder = (ushort)IPAddress.HostToNetworkOrder((short)80);
        private static readonly ushort s_standardHttpsPortNetworkOrder = (ushort)IPAddress.HostToNetworkOrder((short)443);
        private static readonly ushort s_altHttpPortNetworkOrder = (ushort)IPAddress.HostToNetworkOrder((short)8080);
        private static readonly ushort s_altHttpsPortNetworkOrder = (ushort)IPAddress.HostToNetworkOrder((short)8443);
        private static readonly long s_maxInMemoryData = 128000000;

        private static string pathBlockList = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "blocklist.txt");
        private static SortedSet<string> blockList = new SortedSet<string>();
        private static void Intialize()
        {
            // Load block list
            //var lines = File.ReadAllLines(pathBlockList);
            //foreach (var line in lines)
            //{
            //    blockList.Add(line);
            //}
        }
        public static void StartProxyFilter(string pathListBlock = "blocklist.txt")
        {
            Intialize();
            GrantSelfFirewallAccess();

            s_blockPageBytes = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BlockedPage.html"));

            // Let the user decide when to quit with ctrl+c.
            var manualResetEvent = new ManualResetEvent(false);

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                manualResetEvent.Set();
                Console.WriteLine("Shutting Down");
            };

            var cfg = new ProxyServerConfiguration
            {
                AuthorityName = "CitadelCore",
                FirewallCheckCallback = OnFirewallCheck,                            // lọc trước khi request
                NewHttpMessageHandler = OnNewMessage,                               // request trước khi gửi
                HttpExternalRequestHandlerCallback = OnManualFulfillmentCallback,   // Send request
                HttpMessageReplayInspectionCallback = OnReplayInspection,
                HttpMessageWholeBodyInspectionHandler = OnWholeBodyContentInspection, // check nội dung body
                HttpMessageStreamedInspectionHandler = OnStreamedContentInspection,
                BlockExternalProxies = true
            };

            // Just create the server.
            var proxyServer = new WindowsProxyServer(cfg);
            proxyServer.Start(0);

            // And you're up and running.
            Console.WriteLine("Proxy Running");
            Console.WriteLine("Listening for IPv4 HTTP/HTTPS connections on port {0}.", proxyServer.V4HttpEndpoint.Port);
            Console.WriteLine("Listening for IPv6 HTTP/HTTPS connections on port {0}.", proxyServer.V6HttpEndpoint.Port);
            // Don't exit on me yet fam.
            manualResetEvent.WaitOne();

            Console.WriteLine("Exiting.");
            proxyServer.Stop();

        }

        // Send to serrver
        private static HttpClient s_client = new HttpClient(new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            UseCookies = false,
            ClientCertificateOptions = ClientCertificateOption.Automatic,
            AllowAutoRedirect = true,
            Proxy = null
        }, true);

        private static FirewallResponse OnFirewallCheck(FirewallRequest request)
        {
            // Only filter chrome. filter trên mấy trình duyệt này thôi
            var filtering = request.BinaryAbsolutePath.IndexOf("chrome", StringComparison.OrdinalIgnoreCase) != -1;
            filtering |= request.BinaryAbsolutePath.IndexOf("firefox", StringComparison.OrdinalIgnoreCase) != -1;
            filtering |= request.BinaryAbsolutePath.IndexOf("msedge.exe", StringComparison.OrdinalIgnoreCase) != -1;
            //var filtering = true;

            if (filtering)
            {
                if (
                    request.RemotePort == s_standardHttpPortNetworkOrder ||
                    request.RemotePort == s_standardHttpsPortNetworkOrder ||
                    request.RemotePort == s_altHttpPortNetworkOrder ||
                    request.RemotePort == s_altHttpsPortNetworkOrder
                    )
                {
                    // Let's allow chrome to access TCP 80 and 443, but block all other ports.
                    //Console.WriteLine("Filtering application {0} destined for {1}", request.BinaryAbsolutePath, (ushort)IPAddress.HostToNetworkOrder((short)request.RemotePort));
                    return new FirewallResponse(CitadelCore.Net.Proxy.FirewallAction.FilterApplication);
                }
                else
                {
                    // Let's allow chrome to access TCP 80 and 443, but ignore all other
                    // ports. We want to allow non 80/443 requests to go through because
                    // this example now demonstrates the replay API, which will cause
                    // a bunch of browser tabs to open whenever you visit my website.
                    //
                    // If we filtered the replays back through the proxy, who knows
                    // what would happen! Actually that's not true, you'd invoke an infinite
                    // loopback, spawn a ton of browser tabs and then call me a bad programmer.
                    //Console.WriteLine("Ignoring internet for application {0} destined for {1}", request.BinaryAbsolutePath, (ushort)IPAddress.HostToNetworkOrder((short)request.RemotePort));
                    return new FirewallResponse(CitadelCore.Net.Proxy.FirewallAction.DontFilterApplication);
                }
            }

            // For all other applications, just let them access the internet without filtering.
            //Console.WriteLine("Not filtering application {0} destined for {1}", request.BinaryAbsolutePath, (ushort)IPAddress.HostToNetworkOrder((short)request.RemotePort));
            return new FirewallResponse(CitadelCore.Net.Proxy.FirewallAction.DontFilterApplication);
        }

        private static void ForceGoogleSafeSearch(HttpMessageInfo messageInfo)
        {
            // If the host has google in it, we'll append the safe search command.
            if (messageInfo.Url.Host.IndexOf("google.", StringComparison.OrdinalIgnoreCase) > -1)
            {
                // Take everything but query params.
                string newUri = messageInfo.Url.GetLeftPart(UriPartial.Path);

                // Parse the params.
                var queryParams = QueryHelpers.ParseQuery(messageInfo.Url.Query);

                // Iterate over all parsed params.
                foreach (var param in queryParams)
                {
                    // Skip any param named "safe" because who knows, the user might
                    // explicitly have &safe=inative, disabling safe search, so just
                    // ignore anything named this.
                    if (param.Key.Equals("safe", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    // Anything not "safe" param, append to the new URI.
                    foreach (var value in param.Value)
                    {
                        newUri = QueryHelpers.AddQueryString(newUri, param.Key, value);
                    }
                }

                // When we're all done, append safe search enforcement.
                newUri = QueryHelpers.AddQueryString(newUri, "safe", "active");

                // if we end up with a valid URI, overwrite it.
                if (Uri.TryCreate(newUri, UriKind.Absolute, out Uri result))
                {
                    messageInfo.Url = result;
                }
            }
        }

        private static void OnNewMessage(HttpMessageInfo messageInfo)
        {
            void BlockWeb()
            {
                Console.WriteLine("Block website:" + messageInfo.Url.Host);
                messageInfo.MessageType = MessageType.Response;
                messageInfo.ProxyNextAction = ProxyNextAction.DropConnection;
                messageInfo.BodyContentType = "text/html";
                messageInfo.Body = s_blockPageBytes;
                return;
            }

            ForceGoogleSafeSearch(messageInfo);

            // Code Read file each line to sorter set
            if (ConfigInApp.bBlockSocial)
            {
                if(BlockDB.ListSocial.Any(s => s.Contains(messageInfo.Url.Host)))
                {
                    BlockWeb();
                    return;
                }
            }
            if (ConfigInApp.bBlockAdult)
            {
                if (BlockDB.ListAdult.Any(s => s.Contains(messageInfo.Url.Host)))
                {
                    BlockWeb();
                    return;
                }
            }
            if (ConfigInApp.bBlockGame)
            {
                if (BlockDB.ListGameApp.Any(s => s.Contains(messageInfo.Url.Host)))
                {
                    BlockWeb();
                    return;
                }

            }

            if(BlockDB.ListUserHost.Any(s => s.Contains(messageInfo.Url.Host)))
            {
                BlockWeb();
                return;
            }


            //if (blockList.Any(s => s.Contains(messageInfo.Url.Host)))
            //{
            //    Console.WriteLine("Block website:" + messageInfo.Url.Host);
            //    messageInfo.MessageType = MessageType.Response;
            //    messageInfo.ProxyNextAction = ProxyNextAction.DropConnection;
            //    messageInfo.BodyContentType = "text/html";
            //    messageInfo.Body = s_blockPageBytes;
            //    return;
            //}

            // Tiếp tục tới check body html
            messageInfo.ProxyNextAction = ProxyNextAction.AllowAndIgnoreContent;

            // Check phải html không
            if (messageInfo.MessageType == MessageType.Response)
            {
                foreach (string headerName in messageInfo.Headers)
                {
                    if (messageInfo.Headers[headerName].IndexOf("html") != -1)
                    {
                        //Console.WriteLine("Requesting to inspect HTML response for request {0}.", messageInfo.Url.Host);
                        messageInfo.ProxyNextAction = ProxyNextAction.AllowButRequestContentInspection;
                        return;
                    }
                }

                // filter video
                //// The other kind of filtering we want to do here is to monitor video
                //// streams. So, if we find a video content type in a response, we'll subscribe
                //// the very new, and extremely exciting streaming inspection callback!!!!!
                //var contentTypeKey = "Content-Type";
                //var contentType = messageInfo.Headers[contentTypeKey];

                //if (contentType != null && (contentType.IndexOf("video/", StringComparison.OrdinalIgnoreCase) != -1 || contentType.IndexOf("mpeg", StringComparison.OrdinalIgnoreCase) != -1))
                //{
                //    // Means we have a video response coming.
                //    // We want to get the video stream too! Because we have the tools to tell
                //    // if video is naughty or nice!
                //    Console.WriteLine("Requesting to inspect streamed video response.");
                //    messageInfo.ProxyNextAction = ProxyNextAction.AllowButRequestStreamedContentInspection;
                //}
            }
        }

        private static async Task OnManualFulfillmentCallback(HttpMessageInfo messageInfo, HttpContext context)
        {
            // Func này gửi dữ liệu tới server
            // Create the message AFTER we give the user a chance to alter things.
            var requestMsg = new HttpRequestMessage(messageInfo.Method, messageInfo.Url);

            // Ignore failed headers. We don't really care.
            var initialFailedHeaders = requestMsg.PopulateHeaders(messageInfo.Headers, messageInfo.ExemptedHeaders);

            // Make sure we send the body.
            if (context.Request.Body != null)
            {
                if (context.Request.Body != null && (context.Request.Headers.ContainsKey("Transfer-Encoding") || (context.Request.ContentLength.HasValue && context.Request.ContentLength.Value > 0)))
                {
                    // We have a body, but the user doesn't want to inspect it. So,
                    // we'll just set our content to wrap the context's input stream.
                    requestMsg.Content = new StreamContent(context.Request.Body);
                }
            }

            try
            {
                var response = await s_client.SendAsync(requestMsg, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);

                // Blow away all response headers. We wanna clone these now from our upstream request.
                context.Response.ClearAllHeaders();

                // Ensure our client's response status code is set to match ours.
                context.Response.StatusCode = (int)response.StatusCode;

                var upstreamResponseHeaders = response.ExportAllHeaders();

                bool responseHasZeroContentLength = false;
                bool responseIsFixedLength = false;

                foreach (var kvp in upstreamResponseHeaders.ToIHeaderDictionary())
                {
                    foreach (var value in kvp.Value)
                    {
                        if (kvp.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase))
                        {
                            responseIsFixedLength = true;

                            if (value.Length <= 0 && value.Equals("0"))
                            {
                                responseHasZeroContentLength = true;
                            }
                        }
                    }
                }

                // Copy over the upstream headers.
                context.Response.PopulateHeaders(upstreamResponseHeaders, new System.Collections.Generic.HashSet<string>());

                // Copy over the upstream body.
                using (var responseStream = await response?.Content.ReadAsStreamAsync())
                {
                    context.Response.StatusCode = (int)response.StatusCode;
                    context.Response.PopulateHeaders(response.ExportAllHeaders(), new System.Collections.Generic.HashSet<string>());

                    if (!responseHasZeroContentLength && responseIsFixedLength)
                    {
                        using (var ms = new MemoryStream())
                        {
                            await Microsoft.AspNetCore.Http.Extensions.StreamCopyOperation.CopyToAsync(responseStream, ms, s_maxInMemoryData, context.RequestAborted);

                            var responseBody = ms.ToArray();

                            context.Response.Headers.Remove("Content-Length");

                            context.Response.Headers.Add("Content-Length", responseBody.Length.ToString());

                            await context.Response.Body.WriteAsync(responseBody, 0, responseBody.Length);
                        }
                    }
                    else
                    {
                        context.Response.Headers.Remove("Content-Length");

                        if (responseHasZeroContentLength)
                        {
                            context.Response.Headers.Add("Content-Length", "0");
                        }
                        else
                        {
                            await Microsoft.AspNetCore.Http.Extensions.StreamCopyOperation.CopyToAsync(responseStream, context.Response.Body, null, context.RequestAborted);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                while (e != null)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
            }
        }
        private static void GrantSelfFirewallAccess()
        {
            string processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            var hostAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

            // We want to delete all rules that match our process name, so we can create new ones
            // that we know will work.
            var myRules = FirewallManager.Instance.Rules.Where(r => r.Name.Equals(processName, StringComparison.OrdinalIgnoreCase)).ToList();
            if (myRules != null)
            {
                foreach (var rule in myRules)
                {
                    FirewallManager.Instance.Rules.Remove(rule);
                }
            }

            // Allow all inbound and outbound communications from our process.
            var inboundRule = FirewallManager.Instance.CreateApplicationRule(
                FirewallProfiles.Domain | FirewallProfiles.Private | FirewallProfiles.Public,
                processName,
                WindowsFirewallHelper.FirewallAction.Allow, hostAssembly.Location
            );
            inboundRule.Direction = FirewallDirection.Inbound;

            FirewallManager.Instance.Rules.Add(inboundRule);

            var outboundRule = FirewallManager.Instance.CreateApplicationRule(
                FirewallProfiles.Domain | FirewallProfiles.Private | FirewallProfiles.Public,
                processName,
                WindowsFirewallHelper.FirewallAction.Allow, hostAssembly.Location
            );
            outboundRule.Direction = FirewallDirection.Outbound;

            // Add the rules to the manager, which will commit them to Windows.
            FirewallManager.Instance.Rules.Add(outboundRule);
        }

        private static void OnReplayInspection(HttpMessageInfo messageInfo, string replayUrl, HttpReplayTerminationCallback cancellationCallback)
        { }
        private static void OnStreamedContentInspection(HttpMessageInfo messageInfo, StreamOperation operation, Memory<byte> buffer, out bool dropConnection)
        {
            dropConnection = false;
        }
        private static void OnWholeBodyContentInspection(HttpMessageInfo messageInfo)
        { }
    }



}
