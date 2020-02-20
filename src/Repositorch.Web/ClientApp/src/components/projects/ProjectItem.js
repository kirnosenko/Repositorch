import React from 'react'
import { Link } from 'react-router-dom'
import * as signalR from '@aspnet/signalr';
import { useSelector, useDispatch } from 'react-redux';
import { addMapping, updateMapping, removeMapping } from '../../state/mappingActions';
import { Button } from 'reactstrap';
import { YesNoButton } from '../YesNoButton';

const styles = {
	li: {
		display: 'flex',
		justifyContent: 'space-between',
		alignItems: 'center',
		padding: '.5rem 1rem',
		border: '1px solid #ccc',
		borderRadius: '4px',
		marginBottom: '.5rem'
	},
	span: {
		display: 'flex',
		justifyContent: 'flex-end'
	},
	revision: {
		fontFamily: 'monospace'
	}
}

export default function ProjectItem(props) {
	const mapping = useSelector(state => state.mappings[props.name]);
	const progress = mapping !== undefined && mapping.progress !== undefined
		? mapping.progress
		: '';
	const dispatch = useDispatch();

	function openConnection() {
		if (mapping !== undefined) return;

		var connection = new signalR.HubConnectionBuilder()
			.withUrl('/Hubs/Mapping').build();
		connection.start().then(_ => {
			connection.on('Progress', (progress, error, working) => {
				dispatch(updateMapping(props.name, progress, error, working));
			});
			connection.invoke('WatchProject', props.name);
		});
		dispatch(addMapping(props.name, connection));
	}

	function closeConnection() {
		if (mapping === undefined) return;

		mapping.connection.off('Progress');
		mapping.connection.stop();
		dispatch(removeMapping(props.name));
	}

	function startMapping() {
		fetch('api/Mapping/Start/' + props.name, {
			method: 'PUT'
		})
		.then((response) => {
			if (!response.ok) throw new Error(response.status);
			openConnection();
		})
		.catch((e) => {
			console.error(e);
		});
	}

	function stopMapping() {
		fetch('api/Mapping/Stop/' + props.name, {
			method: 'PUT'
		})
		.then((response) => {
			if (!response.ok) throw new Error(response.status);
			closeConnection();
		})
		.catch((e) => {
			console.error(e);
		});
	}

	function switchMapping() {
		mapping === undefined
			? startMapping()
			: stopMapping();
	}

	function removeProject() {
		closeConnection();
		props.removeProject(props.name);
	}

	React.useEffect(() => {
	}, [mapping]);

	return (
		<li style={styles.li}>
			<span>
				<Link to={`/${props.name}`}>{props.name}</Link>
			</span>
			<span style={styles.revision}>
				{progress}
			</span>
			<span style={styles.span}>
				<Button
					color="primary"
					size="sm"
					onClick={() => switchMapping()}>
					{mapping === undefined ? "Start mapping" : "Stop mapping"}</Button>
				&nbsp;
				<Link to={`/edit/${props.name}`}>
					<Button
						color="secondary"
						size="sm">Config...</Button>
				</Link>
				&nbsp;
				<Link to={`/${props.name}`}>
					<Button
						color="secondary"
						size="sm">Browse...</Button>
				</Link>
				&nbsp;
				<YesNoButton
					label="Remove"
					title="Remove project"
					text="Are you sure wanna remove project ?"
					yesAction={removeProject} />
			</span>
		</li>
	)
}
