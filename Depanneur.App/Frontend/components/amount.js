import React from "react";
import Radium from "radium";

const Amount = Radium(props => {
    if (props.amount < 0)
        return <span style={[style.negative, style.nowrap]}>- {(-props.amount).toFixed(2)} $</span>;
    else
        return <span style={style.nowrap}>{props.amount.toFixed(2)} $</span>;
})

const style = {
    negative: {
        color: "#090"
    },
    nowrap: {
        whiteSpace: "nowrap"
    }
}

export default Amount;