import React from 'react';
import { NavItem, NavLink } from 'reactstrap';
import { Link } from 'react-router-dom';
import { useSelector } from 'react-redux';
import ContentToLoad from './ContentToLoad';

export default function NavMenuLinks() {

	const metric = useSelector(state => state.metric);

	function renderMenu(menu) {
		if (menu.length === 0) {
			return (
				<NavItem>
					<NavLink tag={Link} className="text-dark" to="/new">New project</NavLink>
				</NavItem>
			);
		}

		return (
			menu.map(item => {
				return (
					<NavItem key={item.name}>
						<NavLink
							tag={Link}
							className="text-dark"
							to={`/${metric.project}/${item.path}`}>{item.name}</NavLink>
					</NavItem>
				)
			})
		);
	}

	React.useEffect(() => {
	}, [metric]);

	if (metric.path === undefined) {
		return renderMenu([]);
	}

	return (
		<ContentToLoad
			url={`api/Metrics/GetMenu/${encodeURIComponent(metric.path)}`}
			renderData={renderMenu} />
	);
}
