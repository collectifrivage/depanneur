import React from "react";
import Radium from "radium";

function OptionGroup(props) {

    const options = [];

    for (let i = 0; i < props.options.length; i++) {
        const o = props.options[i];
        const style = [styles.option];

        if (i === 0) {
            style.push(styles.firstOption);
        }
        if (i === props.options.length - 1) {
            style.push(styles.lastOption);
        }
        if (props.selectedOption === o.key) {
            style.push(styles.activeOption);
        }

        options.push(<div style={style} key={o.key} onClick={() => props.onOptionSelected(o.key)}>{o.label}</div>);
    }

    return (
        <div style={props.style}>
            {props.label ? <span style={styles.label}>{props.label}</span> : null}
            <div style={styles.optionsContainer}>{options}</div>
        </div>
    );
}

const styles = {
    label: {
        display: "inline-block",
        marginRight: 10
    },
    optionsContainer: {
        display: "inline-block",
        whiteSpace: "nowrap"
    },
    option: {
        display: "inline-block",
        paddingTop: 5,
        paddingBottom: 5,
        paddingLeft: 10,
        paddingRight: 10,
        border: "solid 1px #ccc",
        marginRight: -1,
        cursor: "pointer"
    },
    activeOption: {
        cursor: "default",
        backgroundColor: "#eee",
        fontWeight: "bold",
        boxShadow: "inset 2px 2px 6px -2px rgba(0,0,0,0.15)"
    },
    firstOption: {
        borderTopLeftRadius: 5,
        borderBottomLeftRadius: 5
    },
    lastOption: {
        borderTopRightRadius: 5,
        borderBottomRightRadius: 5
    }
}

export default Radium(OptionGroup);