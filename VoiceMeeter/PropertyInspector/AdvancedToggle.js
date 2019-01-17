var websocket = null,
    uuid = null,
    actionInfo = {},
    inInfo = {},
    runningApps = [],
    isQT = navigator.appVersion.includes('QtWebEngine');

function connectSocket(inPort, inUUID, inRegisterEvent, inInfo, inActionInfo) {
    uuid = inUUID;
    actionInfo = JSON.parse(inActionInfo); // cache the info
    inInfo = JSON.parse(inInfo);
    websocket = new WebSocket('ws://localhost:' + inPort);

    addDynamicStyles(inInfo.colors);

    websocket.onopen = function () {
        var json = {
            event: inRegisterEvent,
            uuid: inUUID
        };

        websocket.send(JSON.stringify(json));

        // Notify the plugin that we are connected
        sendValueToPlugin('propertyInspectorConnected', 'property_inspector');
    };

    websocket.onmessage = function (evt) {
        // Received message from Stream Deck
        var jsonObj = JSON.parse(evt.data);

        if (jsonObj.event === 'sendToPropertyInspector') {
            var payload = jsonObj.payload;

            var mode1Value = document.getElementById('mode1Value');
            mode1Value.value = payload['mode1Value'];

            var mode1Param = document.getElementById('mode1Param');
            mode1Param.value = payload['mode1Param'];

            var mode2Value = document.getElementById('mode2Value');
            mode2Value.value = payload['mode2Value'];

            var userImage1_filename = document.getElementById('userImage1_filename');
            userImage1_filename.innerText = payload['userImage1'];
            if (!userImage1_filename.innerText) {
                userImage1_filename.innerText = "No file...";
            }

            var userImage2_filename = document.getElementById('userImage2_filename');
            userImage2_filename.innerText = payload['userImage2'];
            if (!userImage2_filename.innerText) {
                userImage2_filename.innerText = "No file...";
            }

            var titleType = document.getElementById('titleType');
            titleType.value = payload['titleType'];

            var titleParam = document.getElementById('titleParam');
            titleParam.value = payload['titleParam'];
        }
    };
}

function updateSettings() {
    var mode1Value = document.getElementById('mode1Value');
    var mode1Param = document.getElementById('mode1Param');
    var mode2Value = document.getElementById('mode2Value');
    var titleType       = document.getElementById('titleType');
    var titleParam = document.getElementById('titleParam');
    var userImage1 = document.getElementById('userImage1');
    var userImage2 = document.getElementById('userImage2');
    var userImage1_filename = document.getElementById('userImage1_filename');
    var userImage2_filename = document.getElementById('userImage2_filename');
    

    var payload = {};

    payload.property_inspector = 'updateSettings';
    payload.mode1Value = mode1Value.value;
    payload.mode1Param = mode1Param.value;
    payload.mode2Value = mode2Value.value;
    payload.titleType       = titleType.value;
    payload.titleParam = titleParam.value;
    payload.userImage1 = userImage1.value;
    if (!userImage1.value) {
        // Fetch innerText if file is empty (happens when we lose and regain focus to this key)
        payload.userImage1 = userImage1_filename.innerText;
    }
    else {
        // Set value on initial file selction
        userImage1_filename.innerText = userImage1.value;
    }

    payload.userImage2 = userImage2.value;
    if (!userImage2.value) {
        // Fetch innerText if file is empty (happens when we lose and regain focus to this key)
        payload.userImage2 = userImage2_filename.innerText;
    }
    else {
        // Set value on initial file selction
        userImage2_filename.innerText = userImage2.value;
    }

    
    sendPayloadToPlugin(payload);
}

function sendPayloadToPlugin(payload) {
    if (websocket && (websocket.readyState === 1)) {
        const json = {
            'action': actionInfo['action'],
            'event': 'sendToPlugin',
            'context': uuid,
            'payload': payload
        };
        websocket.send(JSON.stringify(json));
    }
}

// our method to pass values to the plugin
function sendValueToPlugin(value, param) {
    if (websocket && (websocket.readyState === 1)) {
        const json = {
            'action': actionInfo['action'],
            'event': 'sendToPlugin',
            'context': uuid,
            'payload': {
                [param]: value
            }
        };
        websocket.send(JSON.stringify(json));
    }
}

function openWebsite() {
    if (websocket && (websocket.readyState === 1)) {
        const json = {
            'event': 'openUrl',
            'payload': {
                'url': 'https://BarRaider.github.io'
            }
        };
        websocket.send(JSON.stringify(json));
    }
}

if (!isQT) {
    document.addEventListener('DOMContentLoaded', function () {
        initPropertyInspector();
    });
}

window.addEventListener('beforeunload', function (e) {
    e.preventDefault();

    // Notify the plugin we are about to leave
    sendValueToPlugin('propertyInspectorWillDisappear', 'property_inspector');

    // Don't set a returnValue to the event, otherwise Chromium with throw an error.
});

function initPropertyInspector() {
    // Place to add functions
}


function addDynamicStyles(clrs) {
    const node = document.getElementById('#sdpi-dynamic-styles') || document.createElement('style');
    if (!clrs.mouseDownColor) clrs.mouseDownColor = fadeColor(clrs.highlightColor, -100);
    const clr = clrs.highlightColor.slice(0, 7);
    const clr1 = fadeColor(clr, 100);
    const clr2 = fadeColor(clr, 60);
    const metersActiveColor = fadeColor(clr, -60);

    node.setAttribute('id', 'sdpi-dynamic-styles');
    node.innerHTML = `

    input[type="radio"]:checked + label span,
    input[type="checkbox"]:checked + label span {
        background-color: ${clrs.highlightColor};
    }

    input[type="radio"]:active:checked + label span,
    input[type="radio"]:active + label span,
    input[type="checkbox"]:active:checked + label span,
    input[type="checkbox"]:active + label span {
      background-color: ${clrs.mouseDownColor};
    }

    input[type="radio"]:active + label span,
    input[type="checkbox"]:active + label span {
      background-color: ${clrs.buttonPressedBorderColor};
    }

    td.selected,
    td.selected:hover,
    li.selected:hover,
    li.selected {
      color: white;
      background-color: ${clrs.highlightColor};
    }

    .sdpi-file-label > label:active,
    .sdpi-file-label.file:active,
    label.sdpi-file-label:active,
    label.sdpi-file-info:active,
    input[type="file"]::-webkit-file-upload-button:active,
    button:active {
      background-color: ${clrs.buttonPressedBackgroundColor};
      color: ${clrs.buttonPressedTextColor};
      border-color: ${clrs.buttonPressedBorderColor};
    }

    ::-webkit-progress-value,
    meter::-webkit-meter-optimum-value {
        background: linear-gradient(${clr2}, ${clr1} 20%, ${clr} 45%, ${clr} 55%, ${clr2})
    }

    ::-webkit-progress-value:active,
    meter::-webkit-meter-optimum-value:active {
        background: linear-gradient(${clr}, ${clr2} 20%, ${metersActiveColor} 45%, ${metersActiveColor} 55%, ${clr})
    }
    `;
    document.body.appendChild(node);
};

/** UTILITIES */

/*
    Quick utility to lighten or darken a color (doesn't take color-drifting, etc. into account)
    Usage:
    fadeColor('#061261', 100); // will lighten the color
    fadeColor('#200867'), -100); // will darken the color
*/
function fadeColor(col, amt) {
    const min = Math.min, max = Math.max;
    const num = parseInt(col.replace(/#/g, ''), 16);
    const r = min(255, max((num >> 16) + amt, 0));
    const g = min(255, max((num & 0x0000FF) + amt, 0));
    const b = min(255, max(((num >> 8) & 0x00FF) + amt, 0));
    return '#' + (g | (b << 8) | (r << 16)).toString(16).padStart(6, 0);
}
