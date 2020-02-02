import React, { Suspense } from 'react'
import { useDispatch } from 'react-redux';
import { setMetricPath } from '../../state/metricActions';
import Loading from '../Loading';

export default function ProjectBrowse({ match }) {

	const metricPath = match.params[0];
	const MetricComponent = React.lazy(() => import(`../metrics/${metricPath}`));

	const dispatch = useDispatch();
	dispatch(setMetricPath(metricPath));

	return (
		<Suspense fallback={<Loading />}>
			<MetricComponent project={match.params.project} />
		</Suspense>
	);
}
