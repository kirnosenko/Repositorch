import React from 'react';
import DatePicker from "react-datepicker";
import propTypes from 'prop-types';
import { secondsToDate, dateToSeconds } from '../../functions';
import "react-datepicker/dist/react-datepicker.css";

export default function BurndownForm(props) {

	const [settings, setSettings] = React.useState(props.settings);

	function setSetting(name, value) {
		setSettings({
			...settings,
			[name]: value
		});
	}

	function handleChange(evt) {
		setSetting(evt.target.name, evt.target.value);
	}

	async function handleSubmit(event) {
		event.preventDefault();
		props.updateSettings(settings);
	}

	return (
		<form onSubmit={handleSubmit}>
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
					selected={secondsToDate(settings.dateFrom)}
					onChange={(date) => setSetting("dateFrom", dateToSeconds(date))} />
			</div>
			<div className="form-group">
				<div className="heading">Till date</div>
				<DatePicker
					selected={secondsToDate(settings.dateTo)}
					onChange={(date) => setSetting("dateTo", dateToSeconds(date))} />
			</div>
			<button
				type="submit"
				className="btn btn-outline-dark btn-sm">
				Update...
				</button>
		</form>
	);
}

BurndownForm.propTypes = {
	settings: propTypes.object.isRequired,
	updateSettings: propTypes.func.isRequired
};
