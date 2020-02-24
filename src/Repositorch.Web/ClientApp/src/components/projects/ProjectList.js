import React from 'react'
import { Redirect } from 'react-router-dom'
import * as signalR from '@aspnet/signalr';
import { useDispatch } from 'react-redux';
import { updateMapping } from '../../state/mappingActions';
import ContentToLoad from '../ContentToLoad'
import ProjectItem from './ProjectItem'

export default function ProjectList() {

	const [list, setList] = React.useState(null);
	const [connection, setConnection] = React.useState(null);
	const dispatch = useDispatch();
	const styles = {
		ul: {
			listStyle: 'none',
			margin: 0,
			padding: 0
		}
	}

	function removeProject(name) {
		fetch("api/Projects/Remove/".concat(name), {
			method: 'DELETE'
		})
		.then((response) => {
			if (!response.ok) throw new Error(response.status);
			setList(list.filter((x) => x !== name));
		})
		.catch((error) => {
			console.error(error);
		});
	}

	function renderProjectList(data) {
		if (data.length === 0) {
			return <Redirect to='/new' />
		}

		return (
			<ul style={styles.ul}>
				{data.map(projectName => {
					return (
						<ProjectItem
							name={projectName}
							key={projectName}
							removeProject={removeProject} />
					)
				})}
			</ul>
		)
	}

	React.useEffect(() => {
		if (list !== null && connection === null) {
			var nc = new signalR.HubConnectionBuilder()
				.withUrl('/Hubs/Mapping').build();
			nc.start().then(_ => {
				nc.on('Progress', (project, progress, error, working) => {
					dispatch(updateMapping(project, progress, error, working));
				});
			});
			setConnection(nc);
		}
		return () => {
			if (connection !== null) {
				connection.off('Progress');
				connection.stop();
			}
		}
	}, [list, connection]);

	return (
		<ContentToLoad
			url="api/Projects/GetNames"
			renderData={renderProjectList}
			data={list}
			setData={setList} />
	);
}
