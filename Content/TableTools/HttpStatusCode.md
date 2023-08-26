# HTTP状态码对照表

HTTP 状态码是表示一次 HTTP 请求的结果状态，也称为响应状态。本工具提供了常见的 HTTP 状态对照表（中英文），从 1xx 到 5xx，详细介绍了每一种 HTTP 状态的含义以及概念。其中，最常见的 HTTP 返回状态是 200、301、302、403、404、500、503 等状态码。HTTP 状态码是了解网页的重要参考因素之一。

HTTP 状态码（HTTP Status Code）由 RFC 2616 规范定义，是用来表示网页服务器 HTTP 响应状态的 3 位数字代码。下面列出了常见的网页 HTTP 状态码，从 1xx 状态到 5xx 状态，给出了每种状态码的详细说明，请查阅。如需查看指定网页返回的 HTTP 状态，请使用本站提供的 HTTP状态查询 工具。

> 注：下表并未列出不常见的 HTTP 状态，如需了解不常见 HTTP 状态的含义，请参考 RFC 2616 规范。

## 1xx 状态：临时响应
1xx 系列状态码，表示请求已收到，需要请求者继续执行操作的状态代码。

### 100
继续（Continue）
请求者应当继续提出请求。 服务器返回此代码表示已收到请求的第一部分，正在等待其余部
The server has received the request headers and the client should proceed to send the request body.

### 101
切换协议（Switching Protocols）
请求者已要求服务器切换协议，服务器已确认并做好了的切换的准备
The requester has asked the server to switch protocols and the server has agreed to do so.

## 2xx 状态：成功响应
2xx 系列状态码，表示成功处理了请求的状态代码。

### 200
成功（Success）
服务器已成功处理了请求。 通常，这表示服务器提供了请求的网页
Standard response for successful HTTP requests.

### 201
已创建（Created）
请求成功，并且服务器创建了新的资源
The request has been fulfilled, resulting in the creation of a new resource.

### 202
已接受（Accepted）
服务器已接受请求，但尚未处理
The request has been accepted for processing, but the processing has not been completed.

### 203
非授权信息（Non Authoritative Information）
服务器已成功处理了请求，但返回的信息可能来自另一来源
The server is a transforming proxy that received a 200 OK from its origin, but is returning a modified version of the origin's response.
从 HTTP/1.1 开始支持

### 204
无内容（No Content）
服务器成功处理了请求，但没有返回任何内容
The server successfully processed the request and is not returning any content.

### 205
重置内容（Reset Content）
服务器成功处理了请求，但没有返回任何内容。该响应要求浏览器重置它所显示的内容
The server successfully processed the request, but is not returning any content. This response requires that the requester reset the document view.

### 206
部分内容（Partial Content）
由于客户端发送了 range 头信息，服务器成功返回了部分资源
The server is delivering only part of the resource due to a range header sent by the client.
该状态码由 RFC 7233 定义

## 3xx 状态：重定向
3xx 系列状态码，表示要完成请求，需要进一步操作。 通常，这些状态代码用来重定向。

### 300
多种选择（Multiple Choices）
针对不同请求，服务器可执行多种操作。服务器可根据请求者的选择执行对应的操作，或者提供一个操作列表供请求者选择
Indicates that multiple options for the resource from which the client may choose.

### 301
永久移动（Moved Permanently）
请求的网页已永久移动到新位置。对于 GET 或 HEAD 请求，服务器返回此响应时，会自动将请求者转到新位置
Indicates the resource had been moved to another location permanently. This and all future requests should be directed to the given URI.

### 302
资源已找到（临时移动）（Found (Moved Temporarily)）
告诉客户端，请到另一处 URL 获取需要的资源（该状态已被 303 和 307 状态取代）
Tells the client to look at (browse to) another URL (302 code has been superseded by 303 and 307).

### 303
查看其他位置（See Other）
请求者应当对不同的位置使用单独的 GET 请求来获取资源时，服务器返回此代码
The response to the request can be found under another URI using the GET method.
从 HTTP/1.1 开始支持

### 304
资源未修改（Not Modified）
自从上次请求后，网页未做过修改。服务器返回此响应时，不会返回网页内容
Indicates that the resource has not been modified since the version specified by the request headers If-Modified-Since or If-None-Match.
该状态码由 RFC 7232 定义

### 305
使用代理（Use Proxy）
请求者只能使用代理访问所请求的资源。资源地址将包含在响应中（出于安全考虑，许多客户端不会遵守该响应状态）
The requested resource is available only through a proxy, the address for which is provided in the response.
从 HTTP/1.1 开始支持

### 306
切换代理（Switch Proxy）
后续请求应该使用指定的代理（在最新版的 HTTP 规范中，该状态码已经不再使用）
No longer used. Subsequent requests should use the specified proxy.
该状态码已废弃

### 307
临时重定向（Temporary Redirect）
请求者应使用另一个 URL 重新发起一次请求，但后续的请求仍应使用原来的 URL
The request should be repeated with another URI; however, future requests should still use the original URI.
从 HTTP/1.1 开始支持

### 308
永久重定向（Permanent Redirect）
当前请求和后续的请求都应该向另一个 URL 发起请求
The request and all future requests should be repeated using another URI.
该状态码由 RFC 7538 定义

## 4xx 状态：客户端错误
4xx 系列状态码，表示客户端请求可能出现了错误，妨碍了服务器的处理。

### 400
错误请求（Bad Request）
服务器不理解客户端请求的语法，如：请求语法错误、请求体过大以及带有欺骗性的请求路径
The server cannot or will not process the request due to an apparent client error.
### 401
未授权（Unauthorized）
访问的资源要求身份验证，但请求时未提供授权或提供了错误的授权。对于需要登录的网页，服务器可能返回此响应
The resource which authentication is required and not yet been provided (or failed).
该状态码由 RFC 7235 定义

### 402
需要支付信息（Payment Required）
为以后使用保留。通常用于需要提供支付的场景，如数字钞票或在线支付。一个例子是：Google 开发者 API 使用 402 状态来表示超过了每日请求上限的情况
Reserved for future use. Might be used as part of some form of digital cash or micropayment scheme, but that has not yet happened.

### 403
禁止访问（Forbidden）
服务器已收到请求，但拒绝提供服务。出现此状态通常是因为请求者没有足够的权限访问请求的资源
The request contained valid data and was understood by the server, but the server is refusing action. This may be due to the user not having the necessary permissions for a resource or needing an account of some sort, or attempting a prohibited action.

### 404
未找到（Not Found）
服务器找不到请求的网页或资源
The requested resource could not be found but may be available in the future.

### 405
不支持的请求方法（Method Not Allowed）
服务器不支持当前请求方法。比如：某些资源只支持 GET 和 POST 请求，如果发起 HEAD 请求，服务器将返回该状态
A request method is not supported for the requested resource.

### 406
不被接受（Not Acceptable）
服务器无法使用请求的内容特性响应请求的网页，即：不支持请求头部中 Accept 字段对应的内容
The requested resource is capable of generating only content not acceptable according to the Accept headers sent in the request.

### 407
需要代理授权（Proxy Authentication Required）
此状态代码与 401（未授权）类似，但指定请求者应当授权使用代理
The client must first authenticate itself with the proxy.
该状态码由 RFC 7235 定义

### 408
请求超时（Request Timeout）
服务器等候请求时发生超时
The server timed out waiting for the request.

### 409
请求冲突（Conflict）
服务器在完成请求时发生冲突。服务器必须在响应中包含有关冲突的信息
Indicates that the request could not be processed because of conflict in the current state of the resource.

### 410
已删除（Gone）
如果请求的资源已永久删除，服务器就会返回此响应
Indicates that the resource requested is no longer available and will not be available again.

### 411
需要有效长度（Length Required）
服务器无法处理不含有内容长度标头字段（Content-Length）的请求
The request did not specify the length of its content, which is required by the requested resource.

### 412
未满足前提条件（Precondition Failed）
服务器未满足请求者在请求中设置的其中一个前提条件
The server does not meet one of the preconditions that the requester put on the request header fields.
该状态码由 RFC 7232 定义

### 413
请求实体过大（Payload Too Large）
服务器无法处理请求，因为请求实体过大，超出服务器的处理能力
The request is larger than the server is willing or able to process. Previously called "Request Entity Too Large".
该状态码由 RFC 7231 定义

### 414
请求的 URI 过长（URI Too Long）
请求的 URI（通常为网址）过长，服务器无法处理
The URI provided was too long for the server to process. Called "Request-URI Too Long" previously.
该状态码由 RFC 7231 定义

### 415
不支持的媒体类型（Unsupported Media Type）
服务器不支持请求的内容格式（Content-Type）
The request entity has a media type which the server or resource does not support.
该状态码由 RFC 7231 定义

### 416
请求范围不符合要求（Range Not Satisfiable）
客户端请求服务器资源的某一部分，但服务器不能提供该部分内容时，则会返回此状态码
The client has asked for a portion of the file (byte serving), but the server cannot supply that portion.
该状态码由 RFC 7233 定义

### 417
未满足期望值（Expectation Failed）
服务器不能满足 "Expection" 请求头部的要求时返回该状态码
The server cannot meet the requirements of the Expect request-header field.

### 421
地址错误的请求（Misdirected Request）
当请求被定向到一个不能输出响应的服务器时，则会返回该状态
The request was directed at a server that is not able to produce a response.
该状态码由 RFC 7540 定义

### 422
无法处理的实体（Unprocessable Entity）
请求格式正确，但由于语义错误，导致无法响应请求
The request was well-formed but was unable to be followed due to semantic errors.
该状态码由 RFC 4918 定义

### 423
被锁定（Locked）
正在访问的资源已被锁定
The resource that is being accessed is locked.
该状态码由 RFC 4918 定义

### 424
依赖请求失败（Failed Dependency）
由于之前的某个请求（即依赖的请求）发生错误，导致当前请求失败
The request failed because it depended on another request and that request failed.
该状态码由 RFC 4918 定义

## 5xx 状态：服务器错误
5xx 系列状态码，表示服务器在尝试处理请求时发生内部错误。 这些错误可能是服务器本身的错误，而不是请求出错。

### 500
服务器内部错误（Internal Server Error）
服务器内部发生错误，无法完成请求
A generic error message, given when an unexpected condition was encountered and no more specific message is suitable.

### 501
尚未实现（Not Implemented）
服务器尚不具备完成请求的条件：要么无法识别请求方法；要么缺乏完成请求的能力
The server either does not recognize the request method, or it lacks the ability to fulfil the request.

### 502
错误网关（Bad Gateway）
服务器作为网关或代理，从上游服务器收到无效响应
The server was acting as a gateway or proxy and received an invalid response from the upstream server.

### 503
服务不可用（Service Unavailable）
服务器目前无法使用（由于超载或停机维护）。通常，这只是一个暂时的状态
The server can't handle the request (because it is overloaded or down for maintenance). Generally, this is a temporary state.

### 504
网关超时（Gateway Timeout）
服务器作为网关或代理，但是没有及时从上游服务器收到请求
The server was acting as a gateway or proxy and did not receive a timely response from the upstream server.

### 505
不支持的 HTTP 协议版本（HTTP Version Not Supported）
服务器不支持请求中所用的 HTTP 协议版本
The server does not support the HTTP protocol version used in the request.

HTTP 状态码是网页服务器响应的最重要的参考指标之一，对网站用户体验、搜索引擎优化、三方 API 设计有巨大的影响。作为程序开发者以及网站管理员，需要了解并熟悉这些常见 HTTP 状态码的含义，并合理地应用到日常 web 开发中。