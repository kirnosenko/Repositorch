import React from 'react';
import { NavItem, NavLink } from 'reactstrap';
import { Link } from 'react-router-dom';
import { useSelector } from 'react-redux';
import ContentToLoad from './ContentToLoad';

export default function NavMenuLinks() {

	const metric = useSelector(state => state.metric);
	const [path, setPath] = React.useState(metric.path);
	const [data, setData] = React.useState(null);
	
	function updateMenu(e, newPath) {
		e.preventDefault();
		setPath(newPath);
	}

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
				var name = item.isMetric
					? item.name
					: <b>{item.name !== undefined ? item.name : "↑"}</b>;
				return (
					<NavItem key={item.path}>
						<NavLink
							tag={Link}
							className="text-dark"
							onClick={e => !item.isMetric ? updateMenu(e, item.path) : {}}
							to={`/${metric.project}${item.path}`}>{name}</NavLink>
					</NavItem>
				)
			})
		);
	}

	React.useEffect(() => {
		setPath(metric.path);
	}, [metric]);

	React.useEffect(() => {
		setData(null);
	}, [path]);

	if (path === undefined) {
		return renderMenu([]);
	}

	return (
		<ContentToLoad
			url={`api/Metrics/GetMenu/${encodeURIComponent(path)}`}
			renderData={renderMenu}
			noloading={true}
			data={data}
			setData={setData} />
	);
}
