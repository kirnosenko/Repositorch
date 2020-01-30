import React from 'react';
import { NavItem, NavLink } from 'reactstrap';
import { Link } from 'react-router-dom';
import ContentToLoad from './ContentToLoad';

export default function NavMenuLinks() {

	function renderMenu(names) {
		if (names.lenght === 0) {
			return (
				<NavItem>
					<NavLink tag={Link} className="text-dark" to="/new">New project</NavLink>
				</NavItem>
			);
		}

		return (
			names.map(name => {
				return (
					<NavItem key={name}>
						<NavLink
							tag={Link}
							className="text-dark"
							to={`/${name}`}>{name}</NavLink>
					</NavItem>
				)
			})
		);
	}

	return (
		<ContentToLoad
			url={`api/Metrics/GetNames`}
			renderData={renderMenu} />
	);
}
