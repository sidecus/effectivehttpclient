HttpClient in .Net is awesome, and it appears very easy to use. However you might or might know that it can lead to several issues including socket exhaustion and difficulties to mock in unit testing. Even when you are aware that you should try to reuse the client, it might cause other issues.

Here is a good read about [socket exhaustion](https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/) caused by inappropriate use of HttpClient.

Here is another read on potential [issues related to DNS](http://byterot.blogspot.com/2016/07/singleton-httpclient-dns.html) when you use it as a simple singleton.

It requires a lot of careful thoughts to really use it right.

This repo tries to make it easy to use. At the same time it enforces patterns to avoid the typical HTTPClient gotchas especially in environments with extensive outbound HTTP calls.

Here are a few principles I tried to follow:
1. Keep similar interface as HttpClient for ease of use
2. Automatic lifecycle management with default behaviors and can easily be updated
3. Encapsulates HttpClient to avoid potential issues, while still leaving enough flexibility
4. Some new C#/.net core 2 features are used, but try to keep it easy to downgrade to older .Net framework when needed
5. Utility classes are generic and can be reused as appropriate

This is a project in progress so if you want to help, feel free to send a PR.
