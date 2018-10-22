// Very simple event system. Probably buggy, but works for our needs.
var callbacks = {};

function registerCallback(event, handler) {
  if (!callbacks[event]) {
    callbacks[event] = [];
  }

  callbacks[event].push(handler);
}

function unregisterCallback(event, handler) {
  if (!callbacks[event]) {
    return;
  }

  callbacks[event] = callbacks[event].filter(cb => cb !== handler);
}

function trigger(event, args) {
  if (!callbacks[event]) {
    return;
  }

  for (let cb of callbacks[event]) {
    cb(args);
  }
}

export default {
  on: registerCallback,
  off: unregisterCallback,
  trigger: trigger
};
