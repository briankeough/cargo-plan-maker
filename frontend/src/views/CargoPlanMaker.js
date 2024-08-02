import * as React from 'react';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Container from '@mui/material/Container';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';
import MenuItem from '@mui/material/MenuItem';
import FormControl from '@mui/material/FormControl';
import Select from '@mui/material/Select';
import LinearProgress from '@mui/material/LinearProgress';
import CircularProgress from '@mui/material/CircularProgress';
import Modal from '@mui/material/Modal';
import IconButton from '@mui/material/IconButton';
import ErrorIcon from '@mui/icons-material/Error'
import DownloadIcon from '@mui/icons-material/Download';
import SettingsIcon from '@mui/icons-material/Settings';
import ClearIcon from '@mui/icons-material/Clear';
import { DataGrid } from '@mui/x-data-grid';
import Snackbar from '@mui/material/Snackbar';
import Alert from '@mui/material/Alert';
import TaskAltIcon from '@mui/icons-material/TaskAlt';
import { apiDomain } from '../config/Config';
import { fileDownloadUri } from '../config/Config';
import { planContainerUriSegment } from '../config/Config';
import { inputFilesContainerUriSegment } from '../config/Config';

export default function CargoPlanMaker() {

  const [runHistoryData, setRunHistoryData] = React.useState([]);
  const [destinationInfo, setDestinationInfo] = React.useState([]);
  const [loading, setLoading] = React.useState(true);
  const [downloadTokenConfig, setDownloadTokenConfig] = React.useState(true);
  
  const [openModal, setOpenModal] = React.useState(false);
  const [openRunModal, setOpenRunModal] = React.useState(false);
  const [runRequestDetails, setRunRequestDetails] = React.useState();
  const [requestor, setRequestor] = React.useState('');
  const [destination, setDestination] = React.useState('NONE');
  const [selectedFile, setSelectedFile] = React.useState();

  const [itemsWithNoSpecs, setItemsWithNoSpecs] = React.useState([]);
  const [duplicateNsns, setDuplicateNsns] = React.useState([]);
  const [inputNsnsMissing, setInputNsnsMissing] = React.useState([]);
  const [validationError, setValidationError] = React.useState('');

  const [numOpenRequests, setNumOpenRequests] = React.useState(false);
  const [showRequestNotification, setShowRequestNotification] = React.useState(false);
  
  const gridColumns = [
    { field: 'requestor', headerName: 'Requestor', flex: 1 },
    { field: 'status', headerName: 'Status', flex: 1, renderCell: (params) => {
      return (
        <>
          { params.row.status === "Completed" && <TaskAltIcon />} 
          { (params.row.status === "Expired" || params.row.status === "Failed") && <ClearIcon /> }
          { params.row.status === "Processing" && <CircularProgress /> } 
          
          &nbsp;&nbsp;{ params.row.status } 
        </>
      );
    }},
    { field: 'formattedTs', headerName: 'Timestamp', flex: 1 },
    { field: 'destinationName', headerName: 'Destination', flex: 1 },
    { field: 'actions', headerName: 'Actions', flex: 1, renderCell: (params) => {
      return (
        <>
          <IconButton aria-label="details"
            color='primary'
            onClick={(e) => showRunDetails(e, params.row)}
          >
            <SettingsIcon />
          </IconButton>
          { params.row.planFile && 
            <a href={fileDownloadUri + planContainerUriSegment + params.row.planFile + downloadTokenConfig.planFile}>
              <IconButton aria-label="download"
                color='primary'
              >
                <DownloadIcon />
              </IconButton>
            </a> 
          }
        </>
      );
    }}
  ];
  
  const resetFields = () => {
    setRequestor('');
    setDestination('NONE');
    setSelectedFile(null);
    setLoading(false);
  }

  const modalStyle = {
    position: 'absolute',
    top: '50%',
    left: '50%',
    transform: 'translate(-50%, -50%)',
    bgcolor: 'background.paper',
    boxShadow: 24,
    p: 6,
    maxHeight: '85%',
    overflow: 'auto'
  };

  const showRunDetails = (e, row) => {
    console.log('show plan details');
    setRunRequestDetails(
      <>
        <Typography id="modal-modal-description" sx={{ mt: 2 }}>
          <b>Requestor:</b> {row.requestor}
        </Typography>
        <Typography id="modal-modal-description" sx={{ mt: 2 }}>
          <b>Timestamp:</b> {row.formattedTs}
        </Typography>
        <Typography id="modal-modal-description" sx={{ mt: 2 }}>
          <b>Status:</b> {row.status}
        </Typography>
        <Typography id="modal-modal-description" sx={{ mt: 2 }}>
          <b>Destination Limit:</b> {row.destinationName}
        </Typography>
        <Typography id="modal-modal-description" sx={{ mt: 2 }}>
          <b>Input File:</b> <a href={fileDownloadUri + inputFilesContainerUriSegment + row.inputFile + downloadTokenConfig.inputFile}>{row.inputFile}</a>
        </Typography>
        <Typography id="modal-modal-description" sx={{ mt: 2 }}>
          <b>Plan File:</b> <a href={fileDownloadUri + planContainerUriSegment + row.planFile + downloadTokenConfig.planFile}>{row.planFile}</a>
        </Typography>
      </>
    )
    setOpenRunModal(true);
  }

  const handleModalClose = () => setOpenModal(false);
  const handleRunModalClose = () => setOpenRunModal(false);

  const destinationMenuItems = destinationInfo.map((item) =>
    <MenuItem value={item.name}>{item.name}</MenuItem>
  );

  const itemsWithNoSpecsList = itemsWithNoSpecs.map((item) =>
    <li>{item}</li>
  );

  const inputNsnsMissingList = inputNsnsMissing.map((item) =>
    <li>{item}</li>
  );

  const duplicateNsnsList = duplicateNsns.map((item) =>
    <li>{item}</li>
  );

  const handleDestinationChange = (event) => {
    setDestination(event.target.value);
  };

  const handleSubmitResponse = (json) => {
    let status = json.status;

    if (status === 'success'){
      console.log(`Successfully uploaded item specs`)
      resetFields();
      getPlanRequests();
 
    } else {
      setLoading(false);
      console.log(`Not successfully saved. Status: ${status}`)
      setItemsWithNoSpecs(json.itemsWithNoSpecs ? json.itemsWithNoSpecs : []);
      setInputNsnsMissing(json.inputNsnsMissing ? json.inputNsnsMissing : []);
      setDuplicateNsns(json.duplicateNsns ? json.duplicateNsns : []);
      setValidationError(json.validationError);
      setOpenModal(true);
    }
  }

  const handleSubmit = async () => {
    setLoading(true);

    const formData = new FormData();
    formData.append("file", selectedFile);
    formData.append("requestor", requestor);
    formData.append("destination", destination);

    try {
      await fetch(`${apiDomain}/api/make-plan`, {
        method: "POST",
        body: formData,
      })
      .then(response => response.json())
      .then(responseData => handleSubmitResponse(responseData))
    
    } catch (error) {
      handleError(error);
    }
  };

  const handleFileUpload = ({ target }) => {
    let file = target.files[0];
    target.value = null;

    setSelectedFile(file);
  }

  const handleError = (error) => {
    console.error(error);
    resetFields();
    setLoading(false);
  }
  
  const handleRunHistoryData = (json) => {
    let openRequestCount = 0;

    for (let i = 0; i < json.length; i++) {
      if (json[i].status === "Processing") {
        openRequestCount++;
      }
    }

    if (openRequestCount < numOpenRequests) {
      setShowRequestNotification(true);
    } else {
      setShowRequestNotification(false);
    }

    setNumOpenRequests(openRequestCount);

    setRunHistoryData(transformRunDataForDisplay(json));
    setLoading(false);
  }

  const handleDestinationInfo = (json) => {
    setDestinationInfo(addIdToEachObjectInArray(json));
  }

  const handleDownloadTokenConfig = (json) => {
    setDownloadTokenConfig(json)
  }

  const getPlanRequests = () => {
    fetch(`${apiDomain}/api/get-run-history`)
      .then(response => response.json())
      .then(json => handleRunHistoryData(json))
      .catch(error => console.error(error));
  }

  const getDestinationInfo = () => {
    fetch(`${apiDomain}/api/get-destination-info`)
      .then(response => response.json())
      .then(json => handleDestinationInfo(json))
      .catch(error => console.error(error));
  }

  const getDownloadToken = () => {
    fetch(`${apiDomain}/api/get-download-token-config`)
      .then(response => response.json())
      .then(json => handleDownloadTokenConfig(json))
      .catch(error => console.error(error));
  }

  const addIdToEachObjectInArray = (arr) => {
    let newArr = [];
    
    arr.forEach((item) => {
      newArr.push({...item, id: item.rowKey})
    })

    return newArr;
  }

  const transformRunDataForDisplay = (arr) => {
    let newArr = [];
    
    arr.forEach((item) => {
      newArr.push({...item, 
        destinationName: item.destination && item.destination.name ? item.destination.name : 'N/A',
        formattedTs : item.timestamp.replace('T', ' ').substr(0, 19)
      })
    })
    return newArr;
  }
  
  const handleNotificationClose = () => {
    setShowRequestNotification(false);
  };

  React.useEffect(() => {
    getDestinationInfo();
    getPlanRequests();
    getDownloadToken();
  }, []);
  
  React.useEffect(() => {
    const interval = setInterval(handlePlanRequestPolling, 5000); 
    return () => clearInterval(interval);
  }, [runHistoryData]);

  const handlePlanRequestPolling = () => {
    let hasOpenRequests = false;

    for (let i = 0; i < runHistoryData.length; i++) {
      if (runHistoryData[i].status === "Processing") {
        hasOpenRequests = true;
        break;
      }
    }
    if (hasOpenRequests) {
      getPlanRequests();
    }
  }

  return (
    <Box
      id="cargo-plan-maker-box"
      sx={(theme) => ({
        width: '100%',
        backgroundImage: 'linear-gradient(#225991, #fdfdff)',
        backgroundSize: '100% 125px',
        backgroundRepeat: 'no-repeat',
      })}
    >
      <Container
        sx={{
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          pt: { xs: 14, sm: 20 },
          pb: { xs: 8, sm: 12 },
        }}
      >
        <Stack 
          spacing={1} 
          useFlexGap sx={{ width: { xs: '100%', sm: '100%' } }}
          direction={{ xs: 'column', sm: 'column' }}
        >
          <Typography
            component="h2"
            variant="h2"
            sx={{
              display: 'flex',
              flexDirection: { xs: 'column', md: 'row' },
              alignSelf: 'center',
              textAlign: 'center',
            }}
          >
            Cargo Plan&nbsp;
            <Typography
              component="span"
              variant="h2"
              sx={{ color: 'primary.main' }}
            >
              Maker
            </Typography>
          </Typography>

          <Stack
            direction={{ xs: 'column', sm: 'row' }}
            alignSelf="left"
            spacing={3}
            useFlexGap
            sx={{ pt: 1, width: { xs: '100%', sm: 'auto' } }}
          >
            {/* form */}
            <Stack
              direction={{ xs: 'column', sm: 'column' }}
              alignSelf="left"
              spacing={1}
              useFlexGap
              sx={{ pt: 4, width: { xs: '100%', sm: '20%' } }}
            >
              <TextField
                id="outlined-basic"
                size="small"
                variant="outlined"
                aria-label="Enter Requestor Name"
                placeholder="Requestor Name"
                inputProps={{
                  autocomplete: 'off',
                  ariaLabel: 'Enter Requestor Name',
                }}
                sx={{ width: { xs: '100%', sm: '219px' } }}
                value={requestor}
                onInput={ e=>setRequestor(e.target.value)}
              />
              <FormControl sx={{ width: { xs: '100%', sm: '219px' } }} size="small">
                <Select
                    variant="outlined"
                    labelId="destination-select"
                    id="destination-select"
                    value={destination}
                    onChange={handleDestinationChange}
                    hiddenlabel
                >
                  <MenuItem value="NONE">Destination Limits:</MenuItem>
                  {destinationInfo && destinationMenuItems.length > 0 && destinationMenuItems}
                </Select>
              </FormControl>
              <Button 
                sx={{ width: { xs: '100%', sm: '219px' } }} 
                variant="outlined" 
                component="label"
              >
                Items To Load
                <input 
                  type="file" 
                  hidden 
                  onChange={handleFileUpload}
                />
              </Button>
              
              {selectedFile && <div>{selectedFile.name}</div>}

              <Button 
                sx={{ width: { xs: '100%', sm: '219px' } }}
                variant="contained" 
                color="primary"
                disableRipple
                onClick={handleSubmit}
              >
                Submit
              </Button>
            </Stack>

            {/* datagrid */}
            <Stack
              direction={{ xs: 'column', sm: 'column' }}
              alignSelf="right"
              spacing={1}
              useFlexGap
              sx={{ pt: 4, width: { xs: '100%', sm: '80%' } }}
            >
              {loading && <LinearProgress /> }
              
              {!loading && 
                <DataGrid 
                  rows={runHistoryData} 
                  columns={gridColumns} 
                  pageSizeOptions={[10, 25, 50]}
                  initialState={{
                    pagination: { paginationModel: { pageSize: 10 } }
                  }}
                />
              }

              <Modal
                open={openModal}
                onClose={handleModalClose}
                aria-labelledby="modal-modal-title"
                aria-describedby="modal-modal-description"
              >
                <Box sx={modalStyle}>
                  <Typography id="modal-modal-title" variant="h4" component="h2">
                    <ErrorIcon  sx={{ fill: '#1976d2', margin: '0px 8px 0px 0px', verticalAlign: 'middle', fontSize: '2em' }} />
                    Problems Found
                  </Typography>
                  {validationError &&
                    <Box>
                      <Typography id="modal-modal-description" sx={{ mt: 2 }}>
                        {validationError}
                      </Typography>
                    </Box>
                  }
                  {itemsWithNoSpecs && itemsWithNoSpecs.length > 0 &&
                    <Box>
                      <Typography id="modal-modal-description" sx={{ mt: 2 }}>
                        NSNs With No Spec Data:
                      </Typography>
                      <ul>{itemsWithNoSpecsList}</ul>
                    </Box>
                  }
                  {inputNsnsMissing && inputNsnsMissing.length > 0 && 
                    <Box>
                      <Typography id="modal-modal-description" sx={{ mt: 2 }}>
                        NSNs With No Qty:
                      </Typography>
                      <ul>{inputNsnsMissingList}</ul>
                    </Box>
                  }
                  {duplicateNsns && duplicateNsns.length > 0 && 
                    <Box>
                      <Typography id="modal-modal-description" sx={{ mt: 2 }}>
                        Duplicate NSN Rows In Input File:
                      </Typography>
                      <ul>{duplicateNsnsList}</ul>
                    </Box>
                  }
                </Box>
              </Modal>

              <Modal
                open={openRunModal}
                onClose={handleRunModalClose}
                aria-labelledby="run-modal"
                aria-describedby="run-modal"
              >
                <Box sx={modalStyle}>
                  <Typography id="modal-modal-title" variant="h4" component="h2">
                    Cargo Plan Details
                  </Typography>
                  {runRequestDetails}
                </Box>
              </Modal>

              <Snackbar 
                anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
                open={showRequestNotification}
                onClose={handleNotificationClose}
                autoHideDuration={5000}
                key={'topright'}
              >
                <Alert
                  onClose={handleNotificationClose}
                  severity="success"
                  variant="filled"
                  sx={{ width: '100%', background: '#1976d2' }}
                >
                  Cargo plan is now complete!
                </Alert>
              </Snackbar>

            </Stack>
          </Stack>
        </Stack>
      </Container>
    </Box>
  );
}