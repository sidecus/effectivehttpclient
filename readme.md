HttpClient in .Net is awesome, and it appears very easy to use.
However you might or might know that it can lead to several issues, for example:, 1. socket exhaustion; 2. hard to mock in unit testing.

Here is a good read about [socket exhaustion](https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/) caused by inappropriate use of HttpClient.

This repo tries to make it easy to use, easy to unit test, while at the same time it tries to avoid the implicit gotchas especially in large scale production environments.

This is a project in progress so if you want to help, feel free to send a PR.
