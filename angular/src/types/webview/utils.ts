export function urlSafeDecode(urlencoded: string): string {
  try {
    const safeURL = urlencoded.replace(/\+/g, '%20');
    return decodeURIComponent(safeURL);
  } catch (e) {
    return urlencoded;
  }
}

export function urlParseQueryString(
  queryString: string
): Record<string, string | null> {
  const params: Record<string, string | null> = {};
  if (!queryString.length) {
    return params;
  }
  const queryStringParams = queryString.split('&');
  let i: number;
  let param: string[];
  let paramName: string;
  let paramValue: string | null;
  for (i = 0; i < queryStringParams.length; i++) {
    param = queryStringParams[i].split('=');
    paramName = urlSafeDecode(param[0]);
    paramValue = param[1] == null ? null : urlSafeDecode(param[1]);
    params[paramName] = paramValue;
  }
  return params;
}

export function urlParseHashParams(
  locationHash: string
): Record<string, string | null> {
  locationHash = locationHash.replace(/^#/, '');
  var params: Record<string, string | null> = {};
  if (!locationHash.length) {
    return params;
  }
  if (locationHash.indexOf('=') < 0 && locationHash.indexOf('?') < 0) {
    params._path = urlSafeDecode(locationHash);
    return params;
  }
  var qIndex = locationHash.indexOf('?');
  if (qIndex >= 0) {
    var pathParam = locationHash.substr(0, qIndex);
    params._path = urlSafeDecode(pathParam);
    locationHash = locationHash.substr(qIndex + 1);
  }
  var query_params = urlParseQueryString(locationHash);
  for (var k in query_params) {
    params[k] = query_params[k];
  }
  return params;
}

// Mezon apps will implement this logic to add service params (e.g. mezonShareScoreUrl) to game URL
export function urlAppendHashParams(url: string, addHash: string): string {
  // url looks like 'https://game.com/path?query=1#hash'
  // addHash looks like 'mezonShareScoreUrl=' + encodeURIComponent('mezonapp://share_game_score?hash=very_long_hash123')

  var ind = url.indexOf('#');
  if (ind < 0) {
    // https://game.com/path -> https://game.com/path#mezonShareScoreUrl=etc
    return url + '#' + addHash;
  }
  var curHash = url.substr(ind + 1);
  if (curHash.indexOf('=') >= 0 || curHash.indexOf('?') >= 0) {
    // https://game.com/#hash=1 -> https://game.com/#hash=1&mezonShareScoreUrl=etc
    // https://game.com/#path?query -> https://game.com/#path?query&mezonShareScoreUrl=etc
    return url + '&' + addHash;
  }
  // https://game.com/#hash -> https://game.com/#hash?mezonShareScoreUrl=etc
  if (curHash.length > 0) {
    return url + '?' + addHash;
  }
  // https://game.com/# -> https://game.com/#mezonShareScoreUrl=etc
  return url + addHash;
}

export function sessionStorageSet<T>(key: string, value: T) {
  try {
    window.sessionStorage.setItem('__mezon__' + key, JSON.stringify(value));
    return true;
  } catch (e) {}
  return false;
}

export function sessionStorageGet(key: string) {
  try {
    const value = window.sessionStorage.getItem('__mezon__' + key);
    return value ? JSON.parse(value) : null;
  } catch (e) {}
  return null;
}

export function setCssProperty(name: string, value: string) {
  var root = document.documentElement;
  if (root && root.style && root.style.setProperty) {
    root.style.setProperty('--mezon-' + name, value);
  }
}
