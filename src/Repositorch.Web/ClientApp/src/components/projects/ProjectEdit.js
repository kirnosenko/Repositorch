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
		fastMergeProcessing: false,
		checkMode: 1
	});
	const [file, setFile] = React.useState('')
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
		const action = project === undefined ? "Create" : "Update";
		const formData = new FormData();
		formData.append('settings', JSON.stringify(settings));
		formData.append('file', file);
		const response = await fetch(`api/Projects/${action}`, {
			method: 'POST',
			body: formData
		});
		if (!response.ok) throw new Error(response.status);
		const errors = await response.json();
		if (Object.keys(errors).length === 0) {
			setSettings(null);
		}
		else {
			setValidation(errors);
			setReadyToSave(true);
		}
	}

	async function exportProject() {
		var blobLink = document.createElement('a');
		blobLink.href = `/api/Projects/Export/${project}`;
		blobLink.setAttribute('download', `${project}.json`);
		blobLink.click();
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
			<div className="form-group" hidden={project !== undefined}>
				<div className="heading">File to import data</div>
				<input
					type="file"
					name="file"
					onChange={e => setFile(e.target.files[0])}	/>
				<small className="form-text text-muted">
					File to import data from. Should be exported from Repositorch first.
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
					Allows mapping of repositories with binary files, 
					symbolic links and git links.
					Don't touch if you are not sure.
				</small>
			</div>
			<div className="form-group form-check">
				<input
					type="checkbox"
					className="form-check-input"
					name="fastMergeProcessing"
					checked={settings.fastMergeProcessing}
					onChange={handleChange} />
				<label className="form-check-label">Use fast merge processing</label>
				<small className="form-text text-muted">
					Allows to speed up merge processing, but will cause 
					incorrect data mapping in some cases.
					Don't touch if you are not sure.
				</small>
			</div>
			<div className="form-group">
				<div className="heading">Check result</div>
				<select
					className="custom-select"
					name="checkMode"
					value={settings.checkMode}
					onChange={handleChange}>
					<option value={0}>Nothing</option>
					<option value={1}>Modified</option>
					<option value={2}>Everything</option>
				</select>
				<small className="form-text text-muted">
					Mapped data checking mode.
					Don't touch if you are not sure.
				</small>
			</div>
			<button
				type="submit"
				className="btn btn-outline-dark btn-sm"
				disabled={!readyToSave}>
				{project === undefined ? "Create project..." : "Update project..."}
			</button>
			&nbsp;
			<button
				type="button"
				className="btn btn-outline-dark btn-sm"
				hidden={project === undefined}
				onClick={() => exportProject()}>
				Export
			</button>
		</form>
	);
}
