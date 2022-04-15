/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

import http from 'k6/http';
import { sleep, check } from 'k6';
import { Counter } from 'k6/metrics';

// A simple counter for http requests

export const requests = new Counter('http_reqs');

export const options = {
    //fake thresholds just to group results by tag
    thresholds: {
        'http_req_duration{endpoint:proxy}': [],
        'http_req_duration{endpoint:assertionconsumer}': []
    },
    maxRedirects: 0,
    insecureSkipTLSVerify: true
};

export function setup() {
    const baseUrl = __ENV.BASE_URL;
    const samlRequest = "";
    const relayState = "efb3f7fa-ff5c-4bf2-b8fd-9c05318c551e"
    const signature = ""
    const queryString = `SAMLRequest=${samlRequest}&RelayState=${relayState}&SigAlg=${sigAlg}&Signature=${signature}`
    const referer = "https://adfs.url/adfs/ls?SAMLRequest=pippo&RelayState=pluto?spidL%3D3%26spidACS%3D1"

    const postBody = {
        "SAMLResponse": "",
        "RelayState": relayState
    }

    return {
        queryString: queryString,
        samlRequest: samlRequest,
        postBody: postBody,
        baseUrl: baseUrl,
        referer: referer
    }
}

export default function(data) {

    const samlRequestResponse = http.get(`${data.baseUrl}/Proxy/Index/Postecom?${data.queryString}`, {
        headers: {
            "Referer": data.referer
        },
        tags: {
            "endpoint": "proxy"
        }
    });

    check(samlRequestResponse, {
        'status is 302': (r) => r.status === 302,
        'SAMLRequests are different': (r) => !r.headers.Location.includes(`SAMLRequest=${data.samlRequest}`)
    });

    //simulating user is logging in on third-party IdP
    sleep(5);

    const assertionResponse = http.post(`${data.baseUrl}/Proxy/assertionconsumer`, data.postBody, {
        tags: {
            "endpoint": "assertionconsumer"
        }
    });

    check(assertionResponse, {
        'status is 200': (r) => r.status === 200,
        'Content-Type is text/html': r => r.headers["Content-Type"].includes("text/html"),
        'xxxForm is present': r => {
            const xxxForm = r.html("form[name=xxxForm]");
            return xxxForm.html();
        },
        'SAMLResponses are different': (r) => {
            const SAMLResponse = r.html().find("input[name=SAMLResponse]").attr("value");
            return SAMLResponse && SAMLResponse !== data.postBody.SAMLResponse
        }
    });
}