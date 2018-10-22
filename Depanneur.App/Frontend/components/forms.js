import React from "react";
import Radium from "radium";

let fieldId = 0;
class FormField extends React.Component {
    constructor() {
        super();
        this.state = {
            id: `input-${++fieldId}`
        };
    }

    render() {
        return (
            <div style={styles.formField}>
                <label style={styles.label} htmlFor={this.state.id}>{this.props.label}</label>
                {React.cloneElement(this.props.children, {id: this.state.id})}
                {this.props.help 
                    ? <p style={styles.helpText}>{this.props.help}</p>
                    : null}
            </div>
        );
    }
}

const TextInput = Radium(props => {
    function onChange(ev) {
        if (props.onValueChanged) {
            props.onValueChanged(ev.target.value);
        }
    }

    return (
        <input id={props.id}
               style={[styles.input, props.style]}
               type={props.type || "text"}
               value={props.value || ""}
               onChange={onChange} />
    );
});

class NumberInput extends React.Component {
    constructor(props) {
        super(props);

        this.regex = this.makeRegex(props.decimals, props.allowNegative);
        this.state = {
            lastGoodValue: ""
        };
    }

    render() {
        let value = this.props.value;
        if (value === null || value === undefined)
            value = "";

        return <input id={this.props.id} 
                      style={[styles.input, {textAlign: "right"}, this.props.style]}
                      type="text" 
                      value={value} 
                      onChange={this.onChange}
                      onBlur={this.onBlur}
                      onKeyDown={this.onKeyDown}
                      onFocus={this.onFocus}
                      autoFocus={this.props.autoFocus} />
    }

    onChange = ev => {
        // Accepter n'importe quel texte pendant qu'on tappe
        this.onValueChanged(ev.target.value);
        
        this.updateLastGoodValue();
    }

    // Valider la valeur lorsqu'on quitte le champ ou qu'on appuie sur ENTER
    onBlur = ev => this.verifyValue();
    onKeyDown = ev => {
        if (ev.keyCode === 13) {
            this.verifyValue();
        }
    }

    onFocus = ev => {
        ev.target.select();
    }

    onValueChanged(value) {
        if (this.props.onValueChanged) {
            this.props.onValueChanged(value);
        }
    }

    verifyValue() {
        let value = this.props.value;
        if (value && value.trim) 
            value = value.trim();

        // Si la valeur est un nombre, on va la converti en float
        if (this.regex.test(value)) {
            this.onValueChanged(this.getNumericValue(this.props.value));
        } 
        // Sinon, on revient à la dernière valeur valide
        else {
            this.onValueChanged(this.state.lastGoodValue);
        }
    }

    updateLastGoodValue() {
        if (this.regex.test(this.props.value)) {
            this.setState({
                lastGoodValue: this.getNumericValue(this.props.value)
            })
        }
    }

    makeRegex(decimals, allowNegative) {
        let negativePart = "";
        let decimalsPart = "";

        if (allowNegative) {
            negativePart = "(-\\s*)?";
        }

        if (decimals === null || decimals === undefined) {
            decimalsPart = "([.,]\\d+)?"
        }
        else if (decimals > 0) {
            decimalsPart = `([.,]\\d{1,${decimals}})?`
        }

        return new RegExp(`^${negativePart}\\d*${decimalsPart}$`);
    }

    getNumericValue(value) {
        if (!value || typeof value === "number") return value;
        return parseFloat(value.trim().replace(/,/, '.').replace(/-\s+/, '-'));
    }
}
NumberInput = Radium(NumberInput);

const Button = Radium(props => {
    var style = [styles.button];
    if (props.disabled === true) {
        style.push(styles.buttonDisabled);
    }
    return <button style={style} type={props.type || "button"} onClick={() => !props.disabled && props.onClick && props.onClick()}>{props.children}</button>
});

const Radio = props => {
    return (
        <label style={styles.radio}>
            <input type="radio" checked={props.checked} onChange={props.onChecked} />
            {props.label}
        </label>
    );
}

const Checkbox = props => {
    return (
        <label style={styles.checkbox}>
            <input type="checkbox" checked={props.checked} onChange={ev => props.onChecked(ev.target.checked)} />
            {props.label}
        </label>
    );
}

const styles = {
    formField: {
        marginBottom: 10
    },
    label: {
        display: "block",
        marginBottom: 5
    },
    input: {
        borderRadius: 5,
        border: "solid 1px #ccc",
        padding: 5,

        ":focus": {
            outline: "none",
            backgroundColor: "#fafafa"
        }
    },
    button: {
        marginRight: 5,
        padding: 10,
        backgroundColor: "#fafafa",
        fontWeight: "bold",
        border: "solid 1px #ccc",
        borderRadius: 5,
        cursor: "pointer",
        outline: "none",

        ":hover": {
            backgroundColor: "#fff"
        }
    },
    buttonDisabled: {
        color: "#ccc",
        cursor: "default",

        ":hover": {
            backgroundColor: "#fafafa"
        }
    },
    helpText: {
        color: "#999",
        marginTop: 5,
        marginBottom: 0
    },
    radio: {
        cursor: "pointer",
        margin: 5,
        display: "inline-flex"
    },
    checkbox: {
        cursor: "pointer",
        margin: 5,
        display: "inline-flex"
    }
}


export {
    FormField,
    NumberInput,
    TextInput,
    Button,
    Radio,
    Checkbox
}