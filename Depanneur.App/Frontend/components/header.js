import React from "react";
import Radium from "radium";

import commonStyles from "../styles";
import CurrentUser from "./current-user";
import Link from "./clean-link";
import Role from "./role";

const styles = {
    main: {
        backgroundColor: "#EEE",
        padding: "10px 0"
    },
    inner: {
        display: "flex",
        flexDirection: "row",
        flexWrap: "wrap",
        justifyContent: "space-between",
        alignItems: "center"
    },
    heading: {
        fontFamily: "'Lobster', cursive",
        fontSize: 28,
        marginRight: 10,
        marginBottom: 5
    },
    navbar: {
        backgroundColor: "#333",
        color: "#FFF"
    },
    navbarBlock: {
        display: "flex",
        flexDirection: "row",
        flexWrap: "wrap"
    },
    navbarLink: {
        padding: "5px 15px",
        borderRight: "solid 1px #666"
    },
    firstNavbarLink: {
        borderLeft: "solid 1px #666"
    }
};

const Header = () => {
    return (
        <div>
            <div style={styles.main}>
                <div style={[commonStyles.block, styles.inner]}>
                    <Link to="/" style={styles.heading}>Dépanneur chez Freud</Link>
                    <CurrentUser />
                </div>
            </div>
            <div style={styles.navbar}>
                <div style={[commonStyles.block, styles.navbarBlock]}>
                    <Link style={[styles.navbarLink, styles.firstNavbarLink]} to="/transactions">Mes transactions</Link>
                    <Role name="BALANCES"><Link style={[styles.navbarLink]} to="/balances">Soldes</Link></Role>
                    <Role name="PRODUCTS"><Link style={[styles.navbarLink]} to="/products">Produits</Link></Role>
                    <Role name="USERS"><Link style={[styles.navbarLink]} to="/users">Utilisateurs</Link></Role>
                </div>
            </div>
        </div>
    )
};

export default Radium(Header);