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

export function fetchHistory() {
    const requestOptions = {
        method: 'GET',
        headers: authHeader()
    };
    return fetch(API_URL + "account/history", requestOptions).then(handleResponse, handleError);
}

export function transaction(body, type) {
    const json_content = { "Content-Type": 'application/json' };
    const requestOptions = {
        method: 'POST',
        headers: { ...authHeader(), ...json_content },
        body: JSON.stringify(body)
    };
    let url =  API_URL + "account/";
    url += type === "buy" ? "purchase" : "sell"; 
    console.log(url);
    return fetch(url, requestOptions).then(handleResponse, handleError);
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