using AmericanVirtual.Weather.Challenge.Common.DTOs;
using AmericanVirtual.Weather.Challenge.Database.Model;
using AutoMapper;
using Profile = AutoMapper.Profile;

namespace AmericanVirtual.Weather.Challenge.CoreAPI.Mapper
{
    public class AutoMapperWrapper : IAutoMapper
    {

        public AutoMapperWrapper()
        {
            _mapper = new AutoMapper.Mapper(GetMapperConfiguration());

        }
        public IMapper _mapper { get; set; }


        public IEnumerable<TOutput> Map<TInput, TOutput>(IEnumerable<TInput> input)
        {
            return _mapper.Map<IEnumerable<TInput>, IEnumerable<TOutput>>(input);
        }

        public TOutput Map<TInput, TOutput>(TInput input)
        {
            return _mapper.Map<TInput, TOutput>(input);
        }

        public TOutput Map<TInput, TOutput>(TInput input, TOutput output)
        {
            return _mapper.Map(input, output);
        }

        private MapperConfiguration GetMapperConfiguration()
        {
            return new MapperConfiguration(x =>
            {

                x.CreateMap<User, UserDTO>();
                x.CreateMap<UserDTO, User>();
            });
        }
    }
}
