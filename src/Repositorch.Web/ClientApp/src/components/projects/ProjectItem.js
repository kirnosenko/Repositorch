import React from 'react'
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

	function isMappingInProgress() {
		return mapping !== undefined && mapping.working === true
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
		isMappingInProgress()
			? stopMapping()
			: startMapping();
	}

	function removeProject() {
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
	if (progress !== '') {
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
							<button
								type="button"
								className="btn btn-outline-dark btn-sm"
								onClick={() => switchMapping()}>
								{isMappingInProgress() ? "Stop mapping" : "Start mapping"}</button>
							&nbsp;
							<Link to={`/edit/${props.name}`}>
								<button
									type="button"
									className="btn btn-outline-dark btn-sm">Config...</button>
							</Link>
							&nbsp;
							<Link to={`/${props.name}`}>
								<button
									type="button"
									className="btn btn-outline-dark btn-sm">Browse...</button>
							</Link>
							&nbsp;
							<YesNoButton
								label="Remove"
								title="Remove project"
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
