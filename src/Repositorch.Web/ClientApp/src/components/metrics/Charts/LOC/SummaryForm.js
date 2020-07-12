import React, { Fragment } from 'react';
import propTypes from 'prop-types';
import SettingsForm from '../../SettingsForm';
import PathText from '../../Widgets/PathText';
import AuthorSelector from '../../Widgets/AuthorSelector';
import FromTillDatesPicker from '../../Widgets/FromTillDatesPicker';

export default function SummaryForm(props) {

	function LocCheckBox(settings, handleChange, title, dataKey) {
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

	function getFormContent(settings, setSetting, handleChange) {
		return (
			<Fragment>
				<div className="form-inline" style={{ marginBottom: '1.5rem' }}>
					{LocCheckBox(settings, handleChange, "LOC total", "locTotal")}
					{LocCheckBox(settings, handleChange, "LOC added", "locAdded")}
					{LocCheckBox(settings, handleChange, "LOC removed", "locRemoved")}
				</div>
				<AuthorSelector
					settings={settings}
					handleChange={handleChange} />
				<PathText
					settings={settings}
					handleChange={handleChange} />
				<FromTillDatesPicker
					settings={settings}
					setSetting={setSetting} />
			</Fragment>
		);
	}

	return (
		<SettingsForm
			settings={props.settings}
			updateSettings={props.updateSettings}
			getFormContent={getFormContent} />
	);
}

SummaryForm.propTypes = {
	settings: propTypes.object.isRequired,
	updateSettings: propTypes.func.isRequired
};
