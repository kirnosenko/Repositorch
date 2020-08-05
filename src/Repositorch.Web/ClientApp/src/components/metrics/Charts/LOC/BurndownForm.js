import React, { Fragment } from 'react';
import propTypes from 'prop-types';
import SettingsForm from '../../SettingsForm';
import PathText from '../../Widgets/PathText';
import FromTillDatesPicker from '../../Widgets/FromTillDatesPicker';

export default function BurndownForm(props) {

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
						<option value="0">Year</option>
						<option value="1">Tag</option>
					</select>
				</div>
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

BurndownForm.propTypes = {
	settings: propTypes.object.isRequired,
	updateSettings: propTypes.func.isRequired
};
