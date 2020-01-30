import React, { Suspense } from 'react'
import Loading from '../Loading';

export default function ProjectBrowse({ match }) {

	const MetricComponent = React.lazy(() => import(`../metrics/${match.params.metric}`));

	return (
		<Suspense fallback={<Loading />}>
			<MetricComponent project={match.params.project} />
		</Suspense>
	);
}
