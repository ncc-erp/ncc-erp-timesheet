// const originalSetCookieValue = abp.utils.setCookieValue;

// abp.utils.setCookieValue = function (key, value, expireDate, path, domain, attributes) {
//     var isEmbeddedApp = window.self !== window.top;

//     attributes = attributes || {};

//     if (isEmbeddedApp) {
//         attributes['SameSite'] = 'None';
//         attributes['Secure'] = true;
//     }

//     originalSetCookieValue.call(this, key, value, expireDate, path, domain, attributes);
// };

abp.auth.setToken = function (authToken, expireDate) {
    localStorage.setItem(abp.auth.tokenCookieName, authToken);
};

abp.auth.getToken = function () {
    return localStorage.getItem(abp.auth.tokenCookieName);
};

abp.auth.clearToken = function () {
    localStorage.removeItem(abp.auth.tokenCookieName);
};