import React from 'react';
import DatePicker from "react-datepicker";
import Moment from 'moment';
import propTypes from 'prop-types';
import "react-datepicker/dist/react-datepicker.css";

export default function SummaryForm(props) {

	const [settings, setSettings] = React.useState(props.settings);
	
	function setSetting(name, value) {
		setSettings({
			...settings,
			[name]: value
		});
	}

	function handleChange(evt) {
		const value = evt.target.type === "checkbox"
			? evt.target.checked
			: evt.target.value;
		setSetting(evt.target.name, value);
	}

	async function handleSubmit(event) {
		event.preventDefault();
		props.updateSettings(settings);
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
					{settings.authors.map(author => {
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
			<div className="form-group">
				<div className="heading">From date</div>
				<DatePicker
					selected={Moment.unix(settings.dateFrom).toDate()}
					onChange={(date) => setSetting("dateFrom", Moment(date).unix())} />
			</div>
			<div className="form-group">
				<div className="heading">Till date</div>
				<DatePicker
					selected={Moment.unix(settings.dateTo).toDate()}
					onChange={(date) => setSetting("dateTo", Moment(date).unix())} />
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
	settings: propTypes.object.isRequired,
	updateSettings: propTypes.func.isRequired
};
