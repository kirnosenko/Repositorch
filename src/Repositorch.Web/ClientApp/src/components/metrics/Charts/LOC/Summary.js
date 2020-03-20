import React, { Fragment } from 'react';
import {
	LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer
} from 'recharts';
import Moment from 'moment';
import MultiMetric from '../../MultiMetric';

export default function Summary(props) {

	const [data, setData] = React.useState(null);
	const [settings, setSettings] = React.useState(null);
	const [authors, setAuthors] = React.useState(null);

	function formatDate(date) {
		return Moment(date).format('YYYY-MM-DD');
	}

	function setFormData(data) {
		setSettings(data.settings);
		setAuthors(data.authors);
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

	async function handleSubmit(event) {
		event.preventDefault();
		setData(null);
	}

	function renderSettings() {

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
						<option value="Alan">Alan</option>
						<option value="Bob">Bob</option>
						<option value="Cris">Cris</option>
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

	function renderData(data) {

		function LocLine(title, dataKey, color) {
			return (
				settings[dataKey] ?
					<Line
						type="monotone"
						name={title}
						dataKey={dataKey}
						stroke={color}
						dot={{ r: 0 }} /> : ""
			);
		}

		return (
			<Fragment>
				{renderSettings()}
				<ResponsiveContainer aspect={2} >
					<LineChart data={data}>
						<CartesianGrid strokeDasharray="3 3" />
						<XAxis dataKey="date" tickFormatter={formatDate} />
						<YAxis />
						<Tooltip />
						<Legend />
						{LocLine("LOC total", "locTotal", "#0000FF")}
						{LocLine("LOC added", "locAdded", "#00FF00")}
						{LocLine("LOC removed", "locRemoved", "#FF0000")}
					</LineChart>
				</ResponsiveContainer>
			</Fragment>
		);
	}

	return (
		<MultiMetric
			title="Lines Of Code Summary"
			projectMetricPath={props.projectMetricPath}
			renderData={renderData}
			getData={() => data}
			setData={setData}
			getSettings={() => settings}
			setFormData={setFormData} />
	);
}
