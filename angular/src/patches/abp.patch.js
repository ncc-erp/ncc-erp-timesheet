
abp.auth.setToken = function (authToken, expireDate) {
    localStorage.setItem(abp.auth.tokenCookieName, authToken);
};

abp.auth.getToken = function () {
    return localStorage.getItem(abp.auth.tokenCookieName);
};

abp.auth.clearToken = function () {
    localStorage.removeItem(abp.auth.tokenCookieName);
};