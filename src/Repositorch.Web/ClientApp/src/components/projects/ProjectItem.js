import React, { Fragment } from 'react'
import { Link } from 'react-router-dom'
import { useSelector } from 'react-redux';
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
	const [readyToSwitch, setReadyToSwitch] = React.useState(true);
	const [readyToUse, setReadyToUse] = React.useState(true);

	function isMappingInProgress() {
		return mapping !== undefined && mapping.time === null
	}

	function startMapping() {
		fetch('api/Mapping/Start/' + props.name, {
			method: 'POST'
		})
		.then((response) => {
			if (!response.ok) throw new Error(response.status);
		})
		.catch((e) => {
			console.error(e);
		});
	}

	function stopMapping() {
		fetch('api/Mapping/Stop/' + props.name, {
			method: 'POST'
		})
		.then((response) => {
			if (!response.ok) throw new Error(response.status);
		})
		.catch((e) => {
			console.error(e);
		});
	}

	function switchMapping() {
		setReadyToSwitch(false);
		setReadyToUse(false);
		isMappingInProgress()
			? stopMapping()
			: startMapping();
	}

	function removeProject() {
		setReadyToUse(false);
		props.removeProject(props.name);
	}

	React.useEffect(() => {
		setReadyToSwitch(true);
		setReadyToUse(!isMappingInProgress());
	}, [mapping]);

	var progress = '';
	var bg = '';
	var errors = '';
	var time = '';
	var style = styles.progress;
	if (mapping !== undefined) {
		progress = mapping.progress || 'Mapping is stopped.';
		if (mapping.time !== null) {
			bg = mapping.errors === null
				? 'bg-warning'
				: (mapping.errors.length > 0 ? 'bg-danger' : 'bg-success');
			style = styles.result;
			time = (
				<Fragment>
					<br />Time: {mapping.time}
				</Fragment>
			);
		}
		if (mapping.errors !== null) {
			if (mapping.errors.length === 0) {
				progress = "Mapping is finished.";
			}
			else {
				errors = mapping.errors.map(error => {
					return (
						<Fragment>
							<br />{error}
						</Fragment>
					)
				})
			}
		}
		progress =
			<div className={`card-footer ${bg}`} style={style}>
				{progress}
				{time}
				{errors}
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
							<button
								type="button"
								className="btn btn-outline-dark btn-sm"
								disabled={!readyToSwitch}
								onClick={() => switchMapping()}>
								{isMappingInProgress() ? "Stop mapping" : "Start mapping"}</button>
							&nbsp;
							<Link to={`/edit/${props.name}`}>
								<button
									type="button"
									className="btn btn-outline-dark btn-sm"
									disabled={!readyToUse}>Config...</button>
							</Link>
							&nbsp;
							<Link to={`/${props.name}`}>
								<button
									type="button"
									className="btn btn-outline-dark btn-sm"
									disabled={!readyToUse}>Browse...</button>
							</Link>
							&nbsp;
							<YesNoButton
								label="Remove"
								title="Remove project"
								disabled={!readyToUse}
								text={`Are you sure wanna remove project ${props.name}?`}
								yesAction={removeProject} />
						</div>
					</div>
				</div>
				{progress}
			</div>
		</li>
	)
}
