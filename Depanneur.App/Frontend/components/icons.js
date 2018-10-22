import React from "react";

const SubscriptionIcon = props => {
    props = Object.assign({width: 15}, props);

    return <img src="/subscribe.svg" {...props} />
}

export {
    SubscriptionIcon
}