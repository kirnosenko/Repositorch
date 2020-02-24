import React from 'react';
import { Redirect } from 'react-router-dom'

export default function ProjectEdit({ match }) {

	const project = match.params.project;
	const [settings, setSettings] = React.useState({
		name: "",
		repositoryPath: "",
		branch: "master",
		useExtendedLog: true,
		checkResult: "1"
	});

	function loadSettings() {
		if (project === undefined || settings === null || project === settings.name) {
			return;
		}

		fetch(`api/Projects/GetSettings/${project}`)
			.then((response) => {
				if (!response.ok) throw new Error(response.status);
				return response.json();
			})
			.then((data) => {
				setSettings(data);
			})
			.catch((error) => {
				console.error(error);
			});
	}

	function handleChange(evt) {
		const value = evt.target.type === "checkbox"
			? evt.target.checked
			: evt.target.value;
		setSettings({
			...settings,
			[evt.target.name]: value
		});
	}

	function handleSubmit(event) {
		event.preventDefault();

		var action = project === undefined ? "Create" : "Update"
		fetch(`api/Projects/${action}`, {
			method: 'POST',
			headers: { 'Content-Type': 'application/json' },
			body: JSON.stringify(settings)
		})
		.then((response) => {
			if (!response.ok) throw new Error(response.status);
			setSettings(null);
		})
		.catch((error) => {
			console.error(error);
		});
	}

	React.useEffect(() => {
		loadSettings();
	});

	if (settings == null) {
		return <Redirect to='/' />
	}
	
	return (
		<div>
			<form onSubmit={handleSubmit}>
				<div className="form-group">
					<div className="heading">Project name</div>
					<input
						className="form-control"
						type="text"
						name="name"
						disabled={project !== undefined}
						value={settings.name}
						onChange={handleChange} />
					<small className="form-text text-muted">
						May consist of letters(A-Z, a-z), digits (0-9) and special characters '-', '.', '_', '~'.
					</small>
				</div>
				<div className="form-group">
					<div className="heading">Repository path</div>
					<input
						className="form-control"
						type="text"
						name="repositoryPath"
						value={settings.repositoryPath}
						onChange={handleChange} />
					<small className="form-text text-muted">
						Path to the repository to work with. Only git repositories are supported at the moment.
					</small>
				</div>
				<div className="form-group">
					<div className="heading">Repository branch</div>
					<input
						className="form-control"
						type="text"
						name="branch"
						value={settings.branch}
						onChange={handleChange} />
					<small className="form-text text-muted">
						Branch in the repository to work with. 'master' for git in most cases.
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
						Mapped data checking mode. Don't touch if you are not sure.
					</small>
				</div>
				<button
					type="button"
					className="btn btn-outline-dark btn-sm">
					{project === undefined ? "Create project..." : "Update project..."}
				</button>
			</form>
		</div>
	);
}
