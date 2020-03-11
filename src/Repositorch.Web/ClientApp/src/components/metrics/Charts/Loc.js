import React, { Fragment } from 'react';
import {
	LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer
} from 'recharts';
import Moment from 'moment';
import Metric from '../Metric';

export default function Loc(props) {

	const [settings, setSettings] = React.useState({
		locTotal: true,
		locAdded: false,
		locRemoved: false
	});

	function formatDate(date) {
		return Moment(date).format('YYYY-MM-DD');
	}

	function handleChange(evt) {
		setSettings({
			...settings,
			[evt.target.name]: evt.target.checked
		});
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
			<form className="form-inline" style={{ marginBottom: '1.5rem' }}>
				{LocCheckBox("LOC total", "locTotal")}
				{LocCheckBox("LOC added", "locAdded")}
				{LocCheckBox("LOC removed", "locRemoved")}
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
		<Metric
			title="Lines Of Code"
			projectMetricPath={props.projectMetricPath}
			renderData={renderData} />
	);
}
