﻿using Microsoft.AspNetCore.Mvc;
using Sample.App.CollectionEnvelope;
using Sample.App.Factories;
using Sample.App.Filters;

namespace Sample.App.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  [IdentityFetcher]
  public class SampleController : BaseController<SampleModel, int>
  {
    private ISampleFactory _commandFactory;
    private IIdentityProvider _identityProvider;

    private int _userId => _identityProvider.UserId;

    public SampleController(
      ISampleFactory commandFactory,
      IIdentityProvider identityProvider)
    {
      _commandFactory = commandFactory;
      _identityProvider = identityProvider;
    }

    public override ActionResult<CollectionEnvelope<SampleModel>> GetAll()
    {
      (var resultEntries, int totalEntries, var error) = _commandFactory.GetAll(_userId, Info.PageSize, Info.SkipPages);

      if (error != null)
      {
        return StatusCode(500, error);
      }

      var collectionEnvelope = GetCollectionEnvelope(resultEntries, totalEntries);
      return Ok(collectionEnvelope);
    }

    public override ActionResult<SampleModel> Get(int documentNr)
    {
      (var entity, var error) = _commandFactory.Get(_userId, documentNr);

      if (error != null)
      {
        return StatusCode(500, error);
      }

      if (entity == null)
        return NotFound();

      return Ok(entity);
    }

    public override ActionResult<SampleModel> Create([FromBody] SampleModel data)
    {
      (var result, var error) = _commandFactory.Create(_userId, data);

      if (error != null)
      {
        return StatusCode(500, error);
      }

      if (result == null)
        return BadRequest();

      return Created(CurrentUrl, result);
    }

    public override ActionResult<SampleModel> Update(int documentNr, [FromBody] SampleModel data)
    {
      if(documentNr != data.DocumentNr)
      {
        return BadRequest();
      }

      (var result, var status, var error) = _commandFactory.Update(_userId, data);

      if (error != null)
      {
        return StatusCode(500, error);
      }

      if (status.HasValue)
      {
        if (status.Value)
        {
          return Ok(result);
        }
        return Created(CurrentUrl, result);
      }

      return StatusCode(500, "Unable to update the data.");
    }

    public override ActionResult<SampleModel> Delete(int documentNr)
    {
      (var result, var error) = _commandFactory.Delete(_userId, documentNr);

      if (error != null)
      {
        return StatusCode(500, error);
      }

      if (result)
      {
        return NoContent();
      }

      return BadRequest();
    }
  }
}
