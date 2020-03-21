import React from 'react';
import propTypes from 'prop-types';

export default function SummaryForm(props) {

	const [settings, setSettings] = React.useState(props.data.settings);
	const authors = props.data.authors;

	function handleChange(evt) {
		const value = evt.target.type === "checkbox"
			? evt.target.checked
			: evt.target.value;
		setSettings({
			...settings,
			[evt.target.name]: value
		});
	}

	async function handleSubmit(event) {
		event.preventDefault();
		props.useData(settings);
	}

	function LocCheckBox(title, dataKey) {
		return (
			<div className="form-group form-check" style={{ marginRight: '.5rem' }}>
				<input
					type="checkbox"
					className="form-check-input"
					name={dataKey}
					checked={settings[dataKey]}
					onChange={handleChange} />
				<label className="form-check-label">{title}</label>
			</div>
		);
	}

	return (
		<form onSubmit={handleSubmit}>
			<div className="form-inline" style={{ marginBottom: '1.5rem' }}>
				{LocCheckBox("LOC total", "locTotal")}
				{LocCheckBox("LOC added", "locAdded")}
				{LocCheckBox("LOC removed", "locRemoved")}
			</div>
			<div className="form-group">
				<div className="heading">Author</div>
				<select
					className="custom-select"
					name="author"
					value={settings.author ?? ""}
					onChange={handleChange} >
					<option value="">Not selected</option>
					{authors.map(author => {
						return <option key={author} value={author}>{author}</option>
					})}
				</select>
			</div>
			<div className="form-group">
				<div className="heading">Path contains</div>
				<input
					type="text"
					name="path"
					value={settings.path}
					onChange={handleChange} />
			</div>
			<button
				type="submit"
				className="btn btn-outline-dark btn-sm">
				Update...
				</button>
		</form>
	);
}

SummaryForm.propTypes = {
	data: propTypes.object.isRequired,
	useData: propTypes.func.isRequired
};
