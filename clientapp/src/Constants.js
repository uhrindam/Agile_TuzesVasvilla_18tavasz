export const SECRET_KEY = "8422F77A-866B-4665-B069-DD69AE3D0D23";
export const API_URL = "https://obudai-api.azurewebsites.net/api/";

export const authHeader = () => ({ "X-Access-Token": SECRET_KEY })

export const currencies = {
    btc: "Bitcoin",
    eth: "Ethereum",
    xrp: "Ripple"
};