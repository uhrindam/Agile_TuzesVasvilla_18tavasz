import { authHeader, API_URL, currencies } from "./Constants";

export function getExchangeRate(currency) {
    const requestOptions = {
        method: 'GET',
        headers: authHeader()
    };
    return fetch(API_URL + "exchange/" + currency, requestOptions).then(handleResponse, handleError);
}

export function fetchBalance() {
    const requestOptions = {
        method: 'GET',
        headers: authHeader()
    };
    return fetch(API_URL + "account", requestOptions).then(handleResponse, handleError);
}

export function buyCrypto(body) {
    const json_content = { "Content-Type": 'application/json' };
    const requestOptions = {
        method: 'POST',
        headers: { ...authHeader(), ...json_content },
        body: JSON.stringify(body)
    }; console.log(requestOptions)
    return fetch(API_URL + "account/purchase", requestOptions).then(handleResponse, handleError);
}

export function sellCrypto(body) {
    const json_content = { "Content-Type": 'application/json' };
    const requestOptions = {
        method: 'POST',
        headers: { ...authHeader(), ...json_content },
        body: JSON.stringify(body)
    }; console.log(requestOptions)
    return fetch(API_URL + "account/sell", requestOptions).then(handleResponse, handleError);
}

export function reset() {
    const requestOptions = {
        method: 'POST',
        headers: authHeader()
    };
    return fetch(API_URL + "account/reset", requestOptions).then(handleResponse, handleError);
}

function handleResponse(response) {
    return new Promise((resolve, reject) => {
        if (response.ok) {
            var contentType = response.headers.get("content-type");
            if (contentType && contentType.includes("application/json")) {
                response.json().then(json => resolve(json));
            } else {
                resolve();
            }
        } else {
            response.text().then(text => { reject(text) });
        }
    });
}

function handleError(error) {
    return Promise.reject(error.message);
}