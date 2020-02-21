import React from 'react'
import { Link } from 'react-router-dom'
import * as signalR from '@aspnet/signalr';
import { useSelector, useDispatch } from 'react-redux';
import { addMapping, updateMapping, removeMapping } from '../../state/mappingActions';
import { Button } from 'reactstrap';
import { YesNoButton } from '../YesNoButton';

const styles = {
	item: {
		border: '1px solid #ccc',
		borderRadius: '4px',
		marginBottom: '.5rem'
	},
	progress: {
		fontFamily: 'monospace'
	},
	result: {
		color: 'white'
	}
}

export default function ProjectItem(props) {
	const mapping = useSelector(state => state.mappings[props.name]);
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

	var progress = '';
	var bg = '';
	var style = styles.progress;
	if (mapping !== undefined) {
		if (mapping.working === undefined) {
			progress = "Preparing for mapping..."
		}
		else {
			progress = mapping.progress || mapping.error || "Mapping is finished.";
			if (!mapping.working) {
				bg = mapping.error === null
					? 'bg-success'
					: 'bg-danger';
				style = styles.result;
			}
		}
	}
	if (progress != '') {
		progress =
			<div className={`card-footer ${bg}`} style={style}>
				{progress}
			</div>;
	}
	
	return (
		<li>
			<div className="card" style={styles.item}>
				<div className="row no-gutters">
					<div className="col-md-4 text-left">
						<div className="card-body">
							<Link to={`/${props.name}`}>{props.name}</Link>
						</div>
					</div>
					<div className="col-md-8 text-right">
						<div className="card-body">
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
						</div>
					</div>
				</div>
				{progress}
			</div>
		</li>
	)
}
