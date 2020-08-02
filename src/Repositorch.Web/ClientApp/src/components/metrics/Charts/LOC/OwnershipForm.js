import React, { Fragment } from 'react';
import propTypes from 'prop-types';
import SettingsForm from '../../SettingsForm';
import PathText from '../../Widgets/PathText';
import FromTillDatesPicker from '../../Widgets/FromTillDatesPicker';

export default function OwnershipForm(props) {

	function getFormContent(settings, setSetting, handleChange) {
		return (
			<Fragment>
				<PathText
					settings={settings}
					handleChange={handleChange} />
				<FromTillDatesPicker
					settings={settings}
					setSetting={setSetting} />
				<div className="form-group">
					<div className="heading">Minimal contribution</div>
					<input
						type="number"
						step="0.01"
						name="minimalContribution"
						value={settings.minimalContribution}
						onChange={handleChange} />
				</div>
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

OwnershipForm.propTypes = {
	settings: propTypes.object.isRequired,
	updateSettings: propTypes.func.isRequired
};
