import React from 'react';
import { NavItem, NavLink } from 'reactstrap';
import { Link } from 'react-router-dom';
import { useSelector } from 'react-redux';
import ContentToLoad from './ContentToLoad';

export default function NavMenuLinks() {

	const metricPath = useSelector(state => state.metric);

	function renderMenu(names) {
		if (names.length === 0) {
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

	React.useEffect(() => {
	}, [metricPath]);

	if (metricPath === null) {
		return renderMenu([]);
	}

	return (
		<ContentToLoad
			url={`api/Metrics/GetMenu/${encodeURIComponent(metricPath)}`}
			renderData={renderMenu} />
	);
}
