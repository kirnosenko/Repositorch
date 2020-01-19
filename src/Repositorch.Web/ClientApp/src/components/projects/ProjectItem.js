import React from 'react'
import * as signalR from '@aspnet/signalr';
import { useSelector, useDispatch } from 'react-redux';
import { addMapping, removeMapping } from '../../state/actions';

const styles = {
    li: {
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        padding: '.5rem 1rem',
        border: '1px solid #ccc',
        borderRadius: '4px',
        marginBottom: '.5rem'
    }
}

export default function ProjectItem(props) {
    const mapping = useSelector(state => state.mappings
        .find(x => x === props.name));
    const dispatch = useDispatch();
    const [connection, setConnection] = React.useState(null);
    const [progress, setProgress] = React.useState('');
    const [mounted, setMounted] = React.useState(false);

    function startWatching(newMapping) {
        if (mapping !== undefined) {
            var newConnection = new signalR.HubConnectionBuilder()
                .withUrl('/Hubs/Mapping').build();
            newConnection.start()
                .then(_ => {
                    newConnection.on('Progress', (current, total) => {
                        if (mounted) {
                            setProgress(current.toString() + ' / ' + total.toString());
                        }
                    });
                    newConnection.invoke('StartWatching', props.name)
                        .then(_ => setConnection(newConnection));
                });
        } else if (newMapping) {
            dispatch(addMapping(props.name));
        }
    }

    function stopWatching() {
        if (connection === null) return;

        connection.invoke('StopWatching', props.name)
            .then(_ => {
                connection.off('Progress');
                connection.stop();
                setConnection(null);
                dispatch(removeMapping(props.name));
            });
    }

    function startMapping() {
        fetch('api/Mapping/Start/' + props.name, {
            method: 'PUT'
        })
        .then((response) => {
            if (!response.ok) throw new Error(response.status);
            startWatching(true);
        })
        .catch((e) => {
            console.error(e);
        });
    }

    function stopMapping() {
        fetch('api/Mapping/Stop/' + props.name, {
            method: 'PUT'
        })
        .then((response) => {
            if (!response.ok) throw new Error(response.status);
            stopWatching();
        })
        .catch((e) => {
            console.error(e);
        });
    }

    function switchMapping() {
        connection === null
            ? startMapping()
            : stopMapping();
    }

    function removeProject() {
        stopWatching(true);
        props.removeProject(props.name);
    }

    React.useEffect(() => {
        setMounted(true);
        startWatching(false);
        return () => {
            setMounted(false);
            stopWatching();
        }
    }, [mapping]);

    return (
        <li style={styles.li}>
            <span>
                {props.name}
            </span>
            <span>
                {progress}
            </span>
            <button
                type="button"
                className="btn btn-primary btn-sm"
                onClick={() => switchMapping()}>
                {connection === null ? "Start mapping..." : "Stop mapping"}
            </button>
            <button
                type="button"
                className="btn btn-danger btn-sm"
                onClick={() => removeProject()}>Remove</button>
        </li>
    )
}
