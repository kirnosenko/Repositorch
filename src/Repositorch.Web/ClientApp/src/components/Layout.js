import React, { Fragment } from 'react';
import { Container } from 'reactstrap';
import { NavMenu } from './NavMenu';
import { Footer } from './Footer';

export function Layout({children}) {
    return (
        <Fragment>
            <NavMenu />
            <Container>
                {children}
            </Container>
            <Footer />
        </Fragment>
    );
}
