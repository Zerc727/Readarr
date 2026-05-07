import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import { fetchMetadataSource, saveMetadataSource, setMetadataSourceValue } from 'Store/Actions/settingsActions';
import createSettingsSectionSelector from 'Store/Selectors/createSettingsSectionSelector';
import MetadataSource from './MetadataSource';

const SECTION = 'metadataSource';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.advancedSettings,
    createSettingsSectionSelector(SECTION),
    (advancedSettings, sectionSettings) => {
      return {
        advancedSettings,
        ...sectionSettings
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchFetchMetadataSource: fetchMetadataSource,
  dispatchSetMetadataSourceValue: setMetadataSourceValue,
  dispatchSaveMetadataSource: saveMetadataSource,
  dispatchClearPendingChanges: clearPendingChanges
};

class MetadataSourceConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      dispatchFetchMetadataSource,
      dispatchSaveMetadataSource,
      onChildMounted
    } = this.props;

    dispatchFetchMetadataSource();
    onChildMounted(dispatchSaveMetadataSource);
  }

  componentDidUpdate(prevProps) {
    const {
      hasPendingChanges,
      isSaving,
      onChildStateChange
    } = this.props;

    if (
      prevProps.isSaving !== isSaving ||
      prevProps.hasPendingChanges !== hasPendingChanges
    ) {
      onChildStateChange({
        isSaving,
        hasPendingChanges
      });
    }
  }

  componentWillUnmount() {
    this.props.dispatchClearPendingChanges({ section: 'settings.metadataSource' });
  }

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.dispatchSetMetadataSourceValue({ name, value });
  };

  //
  // Render

  render() {
    return (
      <MetadataSource
        onInputChange={this.onInputChange}
        {...this.props}
      />
    );
  }
}

MetadataSourceConnector.propTypes = {
  isSaving: PropTypes.bool.isRequired,
  hasPendingChanges: PropTypes.bool.isRequired,
  dispatchFetchMetadataSource: PropTypes.func.isRequired,
  dispatchSetMetadataSourceValue: PropTypes.func.isRequired,
  dispatchSaveMetadataSource: PropTypes.func.isRequired,
  dispatchClearPendingChanges: PropTypes.func.isRequired,
  onChildMounted: PropTypes.func.isRequired,
  onChildStateChange: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(MetadataSourceConnector);
