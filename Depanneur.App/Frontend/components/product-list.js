import React from "react";
import {Link} from "react-router-dom";
import Amount from "./amount";
import {Table, HeaderCell, Row, Cell} from "./tables";
import {Button} from "./forms";
import LinkButton from "./link-button";

const ProductList = props => {

    let i = 0;
    const header = [
        <HeaderCell key={i++}>Nom</HeaderCell>,
        <HeaderCell key={i++} style={styles.right}>Prix</HeaderCell>,
        <HeaderCell key={i++}></HeaderCell>
    ];

    return (
        <Table header={header}>
            {props.products.map(p => <ProductRow key={p.id} product={p} toggleDelete={props.toggleDelete} />)}
        </Table>
    );
}

const ProductRow = props => {

    const p = props.product;

    return (
        <Row style={p.isDeleted ? styles.deleted : null}>
            <Cell>{p.name}</Cell>
            <Cell style={styles.right}><Amount amount={p.price} /></Cell>
            <Cell style={styles.right}>
                {renderTools()}
            </Cell>
        </Row>
    );

    function renderTools() {

        if (p.isDeleted) {
            return <Button onClick={() => props.toggleDelete(p)}>Restaurer</Button>
        } else {
            return (
                <div>
                    <Link to={`/products/${p.id}`}>Modifier</Link><br/>
                    <Link to={`/products/${p.id}/purchases`}>Historique d'achats</Link><br/>
                    <LinkButton onClick={() => props.toggleDelete(p)}>Supprimer</LinkButton>
                </div>
            );
        }

    }

}

const styles = {
    right: {
        textAlign: "right"
    },
    deleted: {
        backgroundColor: "#eee",
        color: "#ccc"
    }
}

export default ProductList;