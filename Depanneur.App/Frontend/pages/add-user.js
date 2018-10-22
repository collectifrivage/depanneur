import gql from "graphql-tag";
import React from "react";
import { withApollo } from "react-apollo";

import ErrorMessage from "../components/error-message";
import { Button, FormField, TextInput } from "../components/forms";


const addUserMutation = gql`
    mutation AddUser($user: UserInput!) {
        addUser(user: $user) {
            id
        }
    }
`;

class AddUser extends React.Component {

    constructor(props) {
        super(props);

        this.state = {
            name: "",
            email: ""
        };
    }

    render() {
        return (
            <div>
                <h1>Ajouter un utilisateur</h1>
                <p>Utilisez ce formulaire pour inscrire une personne n'ayant pas d'adresse courriel @sigmund.ca ou @greatify.co.</p>

                <form onSubmit={this.handleSubmit}>
                    <FormField label="Nom">
                        <TextInput value={this.state.name} style={{width: 400, maxWidth: "100%"}} onValueChanged={v => this.setState({name:v})} />
                    </FormField>
                    <FormField label="Courriel" help="Important: l'adresse doit être liée à un compte Google pour que la personne puisse se connecter.">
                        <TextInput value={this.state.email} type="email" style={{width: 400, maxWidth: "100%"}} onValueChanged={v => this.setState({email:v})} />
                    </FormField>

                    {this.state.error ? <ErrorMessage>{this.state.error}</ErrorMessage> : null}

                    <Button type="submit">Enregistrer</Button>
                    <Button onClick={() => this.goBack()}>Annuler</Button>
                </form>
            </div>
        );
    }

    validate() {
        const {name, email} = this.state;
        let err = null;

        if (!name || name.trim() === "") {
            err = "Le nom est requis.";
        }
        else if (!email || email.trim() === "") {
            err = "Le courriel est requis.";
        }

        this.setState({
            error: err
        });

        return err === null;
    }

    handleSubmit = ev => {
        ev.preventDefault();

        if (this.validate()) {
            const {name, email} = this.state;
            
            this.props.client
                .mutate({mutation: addUserMutation, variables: {user: {name, email}}})
                .then(() => this.goBack());
        }
    }

    goBack() {
        this.props.history.push("/users");
    }

}

export default withApollo(AddUser);