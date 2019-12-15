import React from 'react';
import { Container } from 'reactstrap';
import { NavMenu } from './NavMenu';
import { Footer } from './Footer';

export function Layout({children}) {
    return (
        <div>
            <NavMenu />
            <Container>
                {children}
            </Container>
            <Footer />
        </div>
    );
}
