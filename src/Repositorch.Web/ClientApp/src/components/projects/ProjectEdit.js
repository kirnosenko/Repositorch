import React from 'react';
import { Redirect } from 'react-router-dom'

export default function ProjectEdit({ match }) {

	const project = match.params.project;
	const [storeNames, setStoreNames] = React.useState([]);
	const [repoDirs, setRepoDirs] = React.useState([]);
	const [settings, setSettings] = React.useState({
		name: "",
		storeName: "",
		vcsName: "Git",
		repositoryPath: "",
		branch: "master",
		useExtendedLog: true,
		checkResult: "1"
	});
	const [validation, setValidation] = React.useState({});
	const [readyToSave, setReadyToSave] = React.useState(true);

	function loadSettings() {

		if (project === undefined || settings === null || project === settings.name) {

			if (storeNames.length === 0 && repoDirs.length === 0) {
	
				fetch(`api/Projects/GetDataStoreNames`)
					.then((response) => {
						if (!response.ok) throw new Error(response.status);
						return response.json();
					})
					.then((data) => {
						if (data.length > 0) {
							setStoreNames(data);
						}
					})
					.catch((error) => {
						console.error(error);
					});

				fetch(`api/Projects/GetRepoDirs`)
					.then((response) => {
						if (!response.ok) throw new Error(response.status);
						return response.json();
					})
					.then((data) => {
						if (data.length > 0) {
							setRepoDirs(data);
						}
					})
					.catch((error) => {
						console.error(error);
					});
			}

			return;
		}
		
		fetch(`api/Projects/GetSettings/${project}`)
			.then((response) => {
				if (!response.ok) throw new Error(response.status);
				return response.json();
			})
			.then((data) => {
				setSettings(data);
				setStoreNames([data.storeName]);
				setRepoDirs([data.repositoryPath]);
			})
			.catch((error) => {
				console.error(error);
			});
	}

	function handleChange(evt) {
		const name = evt.target.name;
		const value = evt.target.type === "checkbox"
			? evt.target.checked
			: evt.target.value;
		setSettings((state, props) => ({
			...state,
			[name]: value
		}));
	}

	async function handleSubmit(event) {
		event.preventDefault();

		setReadyToSave(false);
		var action = project === undefined ? "Create" : "Update"
		var response = await fetch(`api/Projects/${action}`, {
			method: 'POST',
			headers: { 'Content-Type': 'application/json' },
			body: JSON.stringify(settings)
		});
		if (!response.ok) throw new Error(response.status);
		var errors = await response.json();
		if (Object.keys(errors).length === 0) {
			setSettings(null);
		}
		else {
			setValidation(errors);
			setReadyToSave(true);
		}
	}

	function validatedClass(name) {
		return validation[name] !== undefined ? 'error' : '';
	}

	function validatedTitle(name) {
		return validation[name] !== undefined ? validation[name] : '';
	}

	React.useEffect(() => {
		loadSettings();
	});

	if (settings == null) {
		return <Redirect to='/' />
	}

	const repositoryPathInput = project !== undefined
		? <input
				className={`form-control ${validatedClass('RepositoryPath')}`}
				title={validatedTitle('RepositoryPath')}
				type="text"
				name="repositoryPath"
				value={settings.repositoryPath}
				onChange={handleChange} />
		: <select
				className={`custom-select ${validatedClass('RepositoryPath')}`}
				title={validatedTitle('RepositoryPath')}
				name="repositoryPath"
				disabled={project !== undefined}
				value={settings.repositoryPath}
				onChange={handleChange}>
				<option value="">Not selected</option>
				{
					repoDirs.map(repoDir => {
						return (
							<option key={repoDir} value={repoDir}>{repoDir}</option>
						);
					})
				}
			</select>
	
	return (
		<form onSubmit={handleSubmit}>
			<div className="form-group">
				<div className="heading">Project name</div>
				<input
					className={`form-control ${validatedClass('Name')}`}
					title={validatedTitle('Name')}
					type="text"
					name="name"
					disabled={project !== undefined}
					value={settings.name}
					onChange={handleChange} />
				<small className="form-text text-muted">
					Name of the project to identify it inside Repositorch.
				</small>
			</div>
			<div className="form-group">
				<div className="heading">Data store</div>
				<select
					className={`custom-select ${validatedClass('StoreName')}`}
					title={validatedTitle('StoreName')}
					name="storeName"
					disabled={project !== undefined}
					value={settings.storeName}
					onChange={handleChange}>
					<option value="">Not selected</option>
					{
						storeNames.map(storeName => {
							return (
								<option key={storeName} value={storeName}>{storeName}</option>
							);
						})
					}
				</select>
				<small className="form-text text-muted">
					Database to keep information from version control system.
				</small>
			</div>
			<div className="form-group">
				<div className="heading">Repository path</div>
				{repositoryPathInput}
				<small className="form-text text-muted">
					Path to the repository assosiated with the project.
					Only git repositories are supported at the moment.
				</small>
			</div>
			<div className="form-group">
				<div className="heading">Repository branch</div>
				<input
					className={`form-control ${validatedClass('Branch')}`}
					title={validatedTitle('Branch')}
					type="text"
					name="branch"
					value={settings.branch}
					onChange={handleChange} />
				<small className="form-text text-muted">
					Branch in the repository to work with.
					'master' for git in most cases.
				</small>
			</div>
			<div className="form-group form-check">
				<input
					type="checkbox"
					className="form-check-input"
					name="useExtendedLog"
					checked={settings.useExtendedLog}
					onChange={handleChange} />
				<label className="form-check-label">Use extended log</label>
				<small className="form-text text-muted">
					Whether to use extended log to allow mapping of repositories
					with symbolic links and git links.
					Don't touch if you are not sure.
				</small>
			</div>
			<div className="form-group">
				<div className="heading">Check result</div>
				<select
					className="custom-select"
					name="checkResult"
					value={settings.checkResult}
					onChange={handleChange}>
					<option value="0">Nothing</option>
					<option value="1">Modified</option>
					<option value="2">Everything</option>
				</select>
				<small className="form-text text-muted">
					Mapped data checking mode.
					Don't touch if you are not sure.
				</small>
			</div>
			<button
				type="submit"
				className="btn btn-outline-dark btn-sm"
				disabled={!readyToSave} >
				{project === undefined ? "Create project..." : "Update project..."}
			</button>
		</form>
	);
}
