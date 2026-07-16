mergeInto(LibraryManager.library, {
  Vibrate_WebGL: function (patternMs) {
    // Check if the browser supports the Vibration API
    if (window.navigator && window.navigator.vibrate) {
      window.navigator.vibrate(patternMs);
    }
  }
});
