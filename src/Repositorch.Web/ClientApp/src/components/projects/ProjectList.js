import React from 'react'
import { Redirect } from 'react-router-dom'
import * as signalR from '@microsoft/signalr';
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

	async function loadProjectList() {
		var response = await fetch("api/Projects/GetNames");
		if (!response.ok) throw new Error(response.status);
		var json = await response.json();
		setList(json);
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
		let isMouted = true;

		async function connect() {
			if (connection === null) {
				var nc = new signalR.HubConnectionBuilder()
					.withUrl('/Hubs/Mapping').build();
				await nc.start();
				if (isMouted) {
					nc.on('Progress', (project, progress, errors, time) => {
						dispatch(updateMapping(project, progress, errors, time));
					});
					setConnection(nc);
				}
				else {
					nc.stop();
				}
			}
		}

		connect();

		return () => {
			isMouted = false;
			if (connection !== null) {
				connection.off('Progress');
				connection.stop();
			}
		}
	}, [connection]);

	return (
		<ContentToLoad
			getData={() => list}
			loadData={loadProjectList}
			renderData={renderProjectList} />
	);
}
