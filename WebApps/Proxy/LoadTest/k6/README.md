# K6 Load Test

This is a **really** simple load test written in k6. It sends a sample SAMLRequest to the `/Proxy/Index/Postecom` endpoint and checks the result. It then waits 5 seconds, simulating the user logging in on the IdP, and then sends a SAMLResponse to the `/Proxy/AssertionConsumer` endpoint checking the result.

Requests are tagged with a `endpoint` key which assume the values of `proxy` and `assertionconsumer`. Then two fake `threshold` on `http_req_duration` metric are set in the global options. In such way, the response times in the result are grouped by the `endpoint` tag.

You can run the test from cli running the following command:
```
k6 run loadtest.js -e BASE_URL=<https://proxy.base.url> --duration "5m" --vus 10
```
where
 - *BASE_URL* is the base url of the SPIDProxy (without the '/' at the end)
 - *--duration* sets the duration of the test
 - *--vus* sets the number of VUs used by K6

 In case you want to run the test based on iterations number and not duration-based, you can replace *--duration* with *--iterations* setting the total number of iterations (across all the VUs) that you want to run.

Sample results:
```
✓ status is 302
     ✓ SAMLRequests are different
     ✓ status is 200
     ✓ Content-Type is text/html
     ✓ xxxForm is present
     ✓ SAMLResponses are different

     checks.............................: 100.00% ✓ 60  ✗ 0  
     data_received......................: 147 kB  2.5 kB/s
     data_sent..........................: 136 kB  2.3 kB/s
     http_req_blocked...................: avg=349.79µs min=0s       med=0s       max=6.99ms  p(90)=0s       p(95)=349.79µs
     http_req_connecting................: avg=50.44µs  min=0s       med=0s       max=1ms     p(90)=0s       p(95)=50.45µs 
     http_req_duration..................: avg=418.99ms min=31.99ms  med=235.09ms max=1.2s    p(90)=935.94ms p(95)=1.14s   
       { endpoint:assertionconsumer }...: avg=794.04ms min=416.19ms med=762.51ms max=1.2s    p(90)=1.14s    p(95)=1.17s   
       { endpoint:proxy }...............: avg=43.95ms  min=31.99ms  med=44.4ms   max=53.99ms p(90)=51.29ms  p(95)=52.64ms 
       { expected_response:true }.......: avg=418.99ms min=31.99ms  med=235.09ms max=1.2s    p(90)=935.94ms p(95)=1.14s   
     http_req_failed....................: 0.00%   ✓ 0   ✗ 20 
     http_req_receiving.................: avg=620.26µs min=0s       med=758µs    max=2.28ms  p(90)=1.19ms   p(95)=1.26ms  
     http_req_sending...................: avg=406.57µs min=0s       med=0s       max=2.01ms  p(90)=1.13ms   p(95)=1.23ms  
     http_req_tls_handshaking...........: avg=249.3µs  min=0s       med=0s       max=4.98ms  p(90)=0s       p(95)=249.3µs 
     http_req_waiting...................: avg=417.97ms min=31.99ms  med=234.49ms max=1.2s    p(90)=934.64ms p(95)=1.14s   
     http_reqs..........................: 20      0.342331/s
     iteration_duration.................: avg=5.84s    min=5.46s    med=5.81s    max=6.26s   p(90)=6.18s    p(95)=6.22s   
     iterations.........................: 10      0.171166/s
     vus................................: 1       min=1 max=1
     vus_max............................: 1       min=1 max=1
```
 For further info, please check K6 docs: https://k6.io/docs/

 # Improvements
 The actual script is using a static SAMLRequest/Referer and a static SAMLResponse. We should vary them throughout the test to obtain a more realistic result. Furthermore, the SAMLResponse is a real one which was issued by `Sielte` on `2021-10-29T09:41:43Z` for the cx `Regione Abruzzo` and contains @fume PII. **The cx related info are public so we aren't leaking any information**.
 To let the SPIDProxy AssertionConsumer endpoint accept this SAMLResponse, you should disable the TechnicalChecks (which doesn't make sense in a load test scenario) or set the appsettings.json as following:

```json
"TechnicalChecks": {
    "SkipTechnicalChecks": false
},
"spid": {
	"AssertionIssueInstantToleranceMins": 99999999 //as big as the difference between UtcNow and 2021-10-29T09:41:43Z
},
"Federator": {
	"SPIDEntityId": "http://adfs.regione.abruzzo.it/adfs/services/trust",
	"EntityId": "http://adfs.regione.abruzzo.it/adfs/services/trust",
	"FederatorAttributeConsumerServiceUrl": "https://adfs.regione.abruzzo.it/adfs/ls"
}
```
Moreover you should host the SPIDProxy on `https://spid.regione.abruzzo.it` otherwise the Recipient check will fail. You could achieve it via hosts file or similar approach. We configured K6 to skip TLS certificate validation.