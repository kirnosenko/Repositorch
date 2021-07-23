import React, { Fragment } from 'react';
import propTypes from 'prop-types';
import SettingsForm from './SettingsForm';
import PathText from './Widgets/PathText';

export default function ActivityForm(props) {

	function getFormContent(settings, setSetting, handleChange) {
		return (
			<Fragment>
				<div className="form-group">
					<div className="heading">Slice type</div>
					<select
						className="custom-select"
						name="slice"
						value={settings.slice}
						onChange={handleChange} >
						<option value="0">Week</option>
						<option value="1">Month</option>
						<option value="2">Year</option>
						<option value="3">Tag</option>
					</select>
				</div>
				<PathText
					settings={settings}
					handleChange={handleChange} />
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

ActivityForm.propTypes = {
	settings: propTypes.object.isRequired,
	updateSettings: propTypes.func.isRequired
};
