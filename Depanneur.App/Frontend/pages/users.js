import gql from "graphql-tag";
import React from "react";
import { Query, withApollo } from "react-apollo";
import { Link } from "react-router-dom";
import store from "store";

import { Button, Checkbox } from "../components/forms";
import Loading from "../components/loading-indicator";
import { Cell, HeaderCell, Row, Table } from "../components/tables";


const usersQuery = gql`
    query UsersQuery($includeDeleted: Boolean) {
        me { id email }
        users(includeDeleted: $includeDeleted) {
            id
            name
            email
            permissions
            isDeleted
        }
    }
`;

const deleteUserMutation = gql`
    mutation DeleteUser($id: String!) {
        user(id: $id) {
            delete {
                id
                isDeleted
            }
        }
    }
`;

const restoreUserMutation = gql`
    mutation RestoreUser($id: String!) {
        user(id: $id) {
            restore {
                id
                isDeleted
            }
        }
    }
`;

const setUserPermissionMutation = gql`
    mutation SetUserPermission($userId: String!, $permission: UserPermission!, $assigned: Boolean!) {
        user(id: $userId) {
            setPermission(permission: $permission, assigned: $assigned) {
                id
                permissions
            }
        }
    }
`;

class Users extends React.Component {

    constructor() {
        super();
        this.state = {
            hideDeleted: store.get("dep.users.hideDeleted") === true
        }
    }

    render() {

        let i = 0;
        const header = [
            <HeaderCell key={i++}></HeaderCell>,
            <HeaderCell key={i++} style={styles.center}>Utilisateurs</HeaderCell>,
            <HeaderCell key={i++} style={styles.center}>Produits</HeaderCell>,
            <HeaderCell key={i++} style={styles.center}>Soldes</HeaderCell>,
            <HeaderCell key={i++}></HeaderCell>
        ];

        return (
            <div>
                <h1>Utilisateurs</h1>

                <p><Link to="/users/add">Ajouter un utilisateur</Link></p>

                <Checkbox label="Masquer les utilisateurs supprimés" checked={this.state.hideDeleted} onChecked={c => this.setHideDeleted(c)} />

                <Query query={usersQuery} variables={{includeDeleted: !this.state.hideDeleted}} fetchPolicy="cache-and-network">
                    {
                        ({error, data, refetch}) => {
                            if (error) return <div>Erreur</div>;
                            if (!data.users) return <Loading />;

                            return (            
                                <Table header={header}>
                                    {data.users.map(u => 
                                        <UserRow key={u.id} 
                                                 user={u} 
                                                 isSelf={u.email === data.me.email} 
                                                 onRoleToggled={role => this.toggleRole(u, role)} 
                                                 toggleDelete={u => this.toggleDelete(u)} />)}
                                </Table>
                            );
                        }
                    }
                </Query>

            </div>
        );
    }

    toggleRole(user, role) {
        const hasRole = user.permissions.indexOf(role) >= 0;

        return this.props.client.mutate({
            mutation: setUserPermissionMutation, 
            variables: {
                userId: user.id,
                permission: role, 
                assigned: !hasRole
            }
        });
    }

    toggleDelete(user) {
        var mutation = user.isDeleted ? restoreUserMutation : deleteUserMutation;
        return this.props.client.mutate({mutation, variables: {id: user.id}});
    }

    setHideDeleted(hide) {
        store.set("dep.users.hideDeleted", hide);
        this.setState({
            hideDeleted: hide
        });
    }

}

const UserRow = props => {
    return (
        <Row style={props.user.isDeleted ? styles.deleted : null}>
            <Cell>{props.user.name}<br/>{props.user.email}</Cell>
            <Cell style={styles.center}>
                {props.isSelf ? null : <RoleToggle user={props.user} role="USERS" onToggle={() => props.onRoleToggled("USERS")} /> }
            </Cell>
            <Cell style={styles.center}>
                <RoleToggle user={props.user} role="PRODUCTS" onToggle={() => props.onRoleToggled("PRODUCTS")} />
            </Cell>
            <Cell style={styles.center}>
                <RoleToggle user={props.user} role="BALANCES" onToggle={() => props.onRoleToggled("BALANCES")} />
            </Cell>
            <Cell style={styles.right}>
                {props.isSelf 
                    ? null 
                    : <Button onClick={() => props.toggleDelete(props.user)}>
                          {props.user.isDeleted ? "Restaurer" : "Supprimer"}
                      </Button>
                }
            </Cell>
        </Row>
    )
}

const RoleToggle = props => {
    const hasRole = props.user.permissions.indexOf(props.role) >= 0;

    return (
        <input type="checkbox" checked={hasRole} disabled={props.user.isDeleted} onChange={() => props.onToggle()} />
    );
}

const styles = {
    center: {
        textAlign: "center"
    },
    right: {
        textAlign: "right"
    },
    deleted: {
        backgroundColor: "#eee",
        color: "#ccc"
    }
}

export default withApollo(Users);