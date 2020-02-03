import React, { Suspense } from 'react'
import { useDispatch } from 'react-redux';
import { clearMetric, setMetric } from '../../state/metricActions';
import Loading from '../Loading';

export default function ProjectBrowse({ match }) {

	const project = match.params.project;
	const metricPath = match.params[0] !== undefined
		? match.params[0]
		: 'Summary';

	const MetricComponent = React.lazy(() => import(`../metrics/${metricPath}`));
	const dispatch = useDispatch();
	dispatch(setMetric(project, metricPath));

	React.useEffect(() => {
		return () => {
			dispatch(clearMetric());
		}
	}, [dispatch]);

	return (
		<Suspense fallback={<Loading />}>
			<MetricComponent
				projectMetricPath={`${project}/${encodeURIComponent(metricPath)}`} />
		</Suspense>
	);
}
